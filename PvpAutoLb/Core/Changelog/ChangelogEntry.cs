namespace PvpAutoLb.Core.Changelog;

internal readonly record struct ChangelogEntry(string Version, string Date, string[] Highlights);
