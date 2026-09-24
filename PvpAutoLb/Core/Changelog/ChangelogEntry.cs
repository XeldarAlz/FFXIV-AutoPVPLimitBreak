using PvpAutoLb.Core.Localization;

namespace PvpAutoLb.Core.Changelog;

internal readonly record struct ChangelogEntry(string Version, string Date, LocString[] Highlights);
