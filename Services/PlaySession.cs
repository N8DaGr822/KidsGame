using KidsGameLauncher.Models;

namespace KidsGameLauncher.Services;

// Play History: The host owns navigation/checkpoints; games keep reporting their detailed round results.
public sealed class PlaySession
{
    private readonly AppDataService _data;
    private readonly TimeProvider _clock;
    private readonly DateTimeOffset _startedAt;
    private readonly bool _external;
    private readonly PlayHistoryEntry _checkpoint;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _hasResult;
    private bool _ended;

    public string ProfileId => _checkpoint.ProfileId;

    public PlaySession(AppDataService data, string profileId, string gameId, bool external, TimeProvider? clock = null)
    {
        _data = data;
        _clock = clock ?? TimeProvider.System;
        _startedAt = _clock.GetUtcNow();
        _external = external;
        _checkpoint = new PlayHistoryEntry
        {
            ProfileId = profileId,
            GameId = gameId,
            PlayedAtUtc = _startedAt.UtcDateTime,
            Status = PlaySessionStatus.InProgress
        };
    }

    public async Task RecordResultAsync(PlayHistoryEntry result)
    {
        await _gate.WaitAsync();
        try
        {
            if (_ended) return;
            result.ProfileId = ProfileId;
            result.GameId = _checkpoint.GameId;
            // Replace the unfinished checkpoint with the first result; retain subsequent round results.
            if (!_hasResult) result.Id = _checkpoint.Id;
            _hasResult = true;
            result.Status = PlaySessionStatus.ResultRecorded;
            await _data.SavePlayHistoryAsync(result);
        }
        finally { _gate.Release(); }
    }

    public Task CheckpointAsync() => SaveCheckpointAsync(ending: false);
    public Task EndAsync() => SaveCheckpointAsync(ending: true);

    private async Task SaveCheckpointAsync(bool ending)
    {
        await _gate.WaitAsync();
        try
        {
            if (_ended) return;
            _ended = ending;
            if (_hasResult) return;
            _checkpoint.ElapsedSeconds = (int)Math.Max(0, (_clock.GetUtcNow() - _startedAt).TotalSeconds);
            if (_checkpoint.ElapsedSeconds == 0) return;
            _checkpoint.Status = ending
                ? (_external ? PlaySessionStatus.Ended : PlaySessionStatus.Abandoned)
                : PlaySessionStatus.InProgress;
            await _data.SavePlayHistoryAsync(_checkpoint);
        }
        finally { _gate.Release(); }
    }
}
