using System.Text.Json;
using KidsGameLauncher.Services;
using Xunit;

namespace KidsGameLauncher.Tests;

public class StorageTests
{
    [Fact]
    public async Task ConcurrentInitialLoadsShareOneReadAndOneCache()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var browser = new BrowserStorage { ReadDelay = gate.Task };
        var data = new AppDataService(browser);
        var first = data.LoadAsync();
        var second = data.LoadAsync();
        Assert.Same(first, second);
        gate.SetResult();
        Assert.Same(await first, await second);
        Assert.Equal(1, browser.Reads);
    }

    [Theory]
    [InlineData("{invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Profiles\":null,\"Games\":[]}")]
    public async Task UnreadableDataIsPreservedAndNeverReseeded(string json)
    {
        var browser = new BrowserStorage();
        browser.Values["kgl_appdata"] = json;
        var data = new AppDataService(browser);
        await data.LoadAsync();
        await data.SaveAsync();
        Assert.True(data.NeedsRecovery);
        Assert.Equal(json, await data.ExportAsync());
        Assert.Equal(json, browser.Values["kgl_appdata"]);
        Assert.Equal(0, browser.Writes);
    }

    [Fact]
    public async Task UnavailableStorageCanBeRetriedWithoutReplacingIt()
    {
        var browser = new BrowserStorage { FailReads = true };
        var data = new AppDataService(browser);
        await data.LoadAsync();
        Assert.True(data.NeedsRecovery);
        browser.FailReads = false;
        await data.RetryLoadAsync();
        Assert.False(data.NeedsRecovery);
        Assert.Contains(await data.GetProfilesAsync(), p => p.Id == "kid");
    }

    [Fact]
    public async Task FailedSaveCanBeExportedAndRetried()
    {
        var browser = new BrowserStorage();
        var data = new AppDataService(browser);
        var saved = await data.LoadAsync();
        browser.FailWrites = true;
        saved.Profiles.Single(p => p.Id == "kid").Name = "Updated";
        await data.SaveAsync();
        Assert.True(data.HasUnsavedChanges);
        Assert.NotNull(data.StorageError);
        Assert.Contains("Updated", await data.ExportAsync());
        Assert.DoesNotContain("Updated", browser.Values["kgl_appdata"]);
        browser.FailWrites = false;
        await data.SaveAsync();
        Assert.False(data.HasUnsavedChanges);
        Assert.Null(data.StorageError);
        Assert.Contains("Updated", browser.Values["kgl_appdata"]);
    }

    [Fact]
    public async Task RestorePreservesPreviousBytesAndReplacesCachedData()
    {
        var browser = new BrowserStorage();
        var data = new AppDataService(browser);
        await data.LoadAsync();
        var old = browser.Values["kgl_appdata"];
        var replacement = old.Replace("Buddy", "Restored");
        Assert.True(await data.ImportAsync(replacement));
        Assert.Equal(old, browser.Values["kgl_appdata_before_restore"]);
        Assert.Contains(await data.GetProfilesAsync(), p => p.Name == "Restored");
    }

    [Fact]
    public async Task FailedOrInvalidRestoreDoesNotReplaceCurrentData()
    {
        var browser = new BrowserStorage();
        var data = new AppDataService(browser);
        await data.LoadAsync();
        var old = browser.Values["kgl_appdata"];
        await Assert.ThrowsAsync<JsonException>(() => data.ImportAsync("{}"));
        browser.FailWrites = true;
        Assert.False(await data.ImportAsync(old.Replace("Buddy", "Restored")));
        Assert.Equal(old, browser.Values["kgl_appdata"]);
        Assert.Contains(await data.GetProfilesAsync(), p => p.Name == "Buddy");
    }

    [Fact]
    public async Task ExportRoundTripsProgressAndAccessRules()
    {
        var data = new AppDataService(new BrowserStorage());
        await data.AddUsageSecondsAsync("kid", 35);
        var restored = AppDataService.ReadBackup(await data.ExportAsync());
        Assert.Equal(35, restored.DailyUsageSeconds["kid"].Values.Single());
        Assert.Equal(["memory"], restored.ProfileGameAccess["kid"]);
    }
}
