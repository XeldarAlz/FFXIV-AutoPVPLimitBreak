using System;

namespace PvpAutoLb.Core.Changelog;

internal static class ChangelogData
{
    public static readonly ChangelogEntry[] Entries =
    [
        new("1.2.0.0", "2026-09-24",
        [
            "Added the Console window to view, filter and copy the plugin's logs",
            "Added this changelog, with a badge whenever an update brings something new",
        ]),
    ];

    public static string LatestVersion => Entries[0].Version;

    // Newest first, so every entry above the one the player last saw is new to them. A version that is no longer
    // listed counts as older than all of them.
    public static int UnseenCount(string lastSeenVersion)
    {
        for (var index = 0; index < Entries.Length; index++)
        {
            if (string.Equals(Entries[index].Version, lastSeenVersion, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return Entries.Length;
    }
}
