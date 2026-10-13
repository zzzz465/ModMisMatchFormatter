using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Steamworks;

// Sets the Workshop item's tags the way RimWorld's in-game uploader does ("Mod" + supportedVersions),
// without uploading files or changing anything else. Run from the repository root.

const uint RimWorldAppId = 294100;
bool dryRun = args.Contains("--dry-run");

var publishedFileId = ulong.Parse(File.ReadAllText("About/PublishedFileId.txt").Trim());
var tags = new List<string> { "Mod" };
tags.AddRange(XDocument.Load("About/About.xml").Root!.Element("supportedVersions")!.Elements("li").Select(li => li.Value.Trim()));

var before = await GetPublishedDetailsAsync(publishedFileId);
Console.WriteLine($"Item {publishedFileId}: {before.Title}");
Console.WriteLine($"  current tags: {string.Join(", ", before.Tags)}");
Console.WriteLine($"  new tags:     {string.Join(", ", tags)}");

if (before.Tags.OrderBy(t => t).SequenceEqual(tags.OrderBy(t => t)))
{
    Console.WriteLine("Tags are already up to date.");
    return 0;
}

if (dryRun)
{
    Console.WriteLine("Dry run: nothing changed.");
    return 0;
}

// The process is not started by Steam, so tell steam_api which app it runs as (instead of a steam_appid.txt
// in the working directory).
Environment.SetEnvironmentVariable("SteamAppId", RimWorldAppId.ToString());
Environment.SetEnvironmentVariable("SteamGameId", RimWorldAppId.ToString());

if (!SteamAPI.Init())
{
    Console.Error.WriteLine("Could not initialise Steam. Is the Steam client running and logged in, and does the account own RimWorld?");
    return 1;
}

try
{
    // Right after Init the client may not have connected this app's session yet.
    var loginDeadline = DateTime.UtcNow.AddSeconds(20);
    while (!SteamUser.BLoggedOn() && DateTime.UtcNow < loginDeadline)
    {
        SteamAPI.RunCallbacks();
        Thread.Sleep(200);
    }
    if (!SteamUser.BLoggedOn())
    {
        Console.Error.WriteLine("The Steam client is not logged on to Steam (offline mode, or signed out by another login).");
        return 1;
    }

    ulong me = SteamUser.GetSteamID().m_SteamID;
    if (me != before.Creator)
    {
        Console.Error.WriteLine($"The Steam client is logged in as {me}, but the item belongs to {before.Creator}.");
        return 1;
    }

    var handle = SteamUGC.StartItemUpdate(new AppId_t(RimWorldAppId), new PublishedFileId_t(publishedFileId));
    if (!SteamUGC.SetItemTags(handle, tags))
    {
        Console.Error.WriteLine("SetItemTags was rejected.");
        return 1;
    }

    SubmitItemUpdateResult_t? result = null;
    bool ioFailure = false;
    using var callResult = CallResult<SubmitItemUpdateResult_t>.Create((r, failed) => { result = r; ioFailure = failed; });
    callResult.Set(SteamUGC.SubmitItemUpdate(handle, null));

    var deadline = DateTime.UtcNow.AddMinutes(2);
    while (result is null && DateTime.UtcNow < deadline)
    {
        SteamAPI.RunCallbacks();
        Thread.Sleep(100);
    }

    if (result is not { } submitted || ioFailure || submitted.m_eResult != EResult.k_EResultOK)
    {
        Console.Error.WriteLine($"Update failed: {(result is { } r ? r.m_eResult.ToString() : "no response")}{(ioFailure ? " (I/O failure)" : "")}");
        return 1;
    }
    if (submitted.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        Console.WriteLine("Note: the account still has to accept the Steam Workshop legal agreement.");
}
finally
{
    SteamAPI.Shutdown();
}

var after = await GetPublishedDetailsAsync(publishedFileId);
Console.WriteLine($"  published tags: {string.Join(", ", after.Tags)}");
return after.Tags.OrderBy(t => t).SequenceEqual(tags.OrderBy(t => t)) ? 0 : 1;

// Public Web API, no key needed: title, creator and tags as Steam currently has them.
static async Task<(string Title, ulong Creator, List<string> Tags)> GetPublishedDetailsAsync(ulong id)
{
    using var http = new HttpClient();
    using var response = await http.PostAsync(
        "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/",
        new FormUrlEncodedContent(new Dictionary<string, string> { ["itemcount"] = "1", ["publishedfileids[0]"] = id.ToString() }));
    response.EnsureSuccessStatusCode();

    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    var details = json.RootElement.GetProperty("response").GetProperty("publishedfiledetails")[0];
    return (
        details.GetProperty("title").GetString() ?? "",
        ulong.Parse(details.GetProperty("creator").GetString()!),
        details.GetProperty("tags").EnumerateArray().Select(t => t.GetProperty("tag").GetString()!).ToList());
}
