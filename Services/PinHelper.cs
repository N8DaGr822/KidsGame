using System.Security.Cryptography;
using System.Text;

namespace KidsGameLauncher.Services;

/// <summary>
/// Hashes the admin PIN before it's stored. This is a deterrent for a
/// kid poking around on a shared tablet, not a real security boundary -
/// there's no server, so anything client-side can ultimately be
/// inspected. Don't reuse this for anything that needs real auth.
/// </summary>
public static class PinHelper
{
    public static string Hash(string pin)
    {
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
