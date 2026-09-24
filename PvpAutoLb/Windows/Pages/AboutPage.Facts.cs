using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace PvpAutoLb.Windows.Pages;

internal sealed partial class AboutPage
{
    private readonly record struct FactCategory(FontAwesomeIcon Icon, LocString Header, Vector4 Color, LocString[] Lines);

    private static readonly FactCategory[] Categories =
    [
        new(FontAwesomeIcon.Heart, L.About.ReminderTitle, Styling.AccentRose, L.About.Reminders),
        new(FontAwesomeIcon.Lightbulb, L.About.FactsTitle, Styling.AccentAmberSoft, L.About.Facts),
        new(FontAwesomeIcon.Star, L.About.QuotesTitle, Styling.AccentMintSoft, L.About.Quotes),
        new(FontAwesomeIcon.GrinBeam, L.About.JokesTitle, Styling.AccentBlueSoft, L.About.Jokes),
    ];

    private static readonly int[][] factBags = new int[Categories.Length][];
    private static readonly int[] factBagPositions = new int[Categories.Length];
    private static readonly int[] factLastServed = new int[Categories.Length];

    private static int factCategory = -1;
    private static int factLine;
    private static bool iconHovered;

    // Hovering the app icon deals a fresh line from the next category, shuffled so nothing repeats until the
    // whole category has been seen.
    private static void IconEasterEgg(Vector2 min, Vector2 max, float scale)
    {
        if (!Hit.HoveringRect(min, max))
        {
            iconHovered = false;
            return;
        }

        if (!iconHovered)
        {
            iconHovered = true;
            factCategory = (factCategory + 1) % Categories.Length;
            factLine = NextLineInCategory(factCategory);
        }

        var category = Categories[Math.Max(0, factCategory)];
        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        using (Tooltip.Begin())
        {
            using (Fonts.PushIcon())
            using (ImRaii.PushColor(ImGuiCol.Text, category.Color))
            {
                ImGui.TextUnformatted(category.Icon.ToIconString());
            }

            ImGui.SameLine(0, 8f * scale);
            using (ImRaii.PushColor(ImGuiCol.Text, category.Color))
            {
                ImGui.TextUnformatted(Loc.T(category.Header));
            }

            ImGui.Spacing();
            Tooltip.Text(Loc.T(category.Lines[factLine]));
        }
    }

    private static int NextLineInCategory(int category)
    {
        var count = Categories[category].Lines.Length;
        if (factBags[category] == null || factBagPositions[category] >= count)
        {
            var avoidFirst = factBags[category] == null ? -1 : factLastServed[category];
            factBags[category] = Shuffle(count, avoidFirst);
            factBagPositions[category] = 0;
        }

        var line = factBags[category][factBagPositions[category]++];
        factLastServed[category] = line;
        return line;
    }

    private static int[] Shuffle(int count, int avoidFirst)
    {
        var order = new int[count];
        for (var index = 0; index < count; index++)
        {
            order[index] = index;
        }

        for (var index = count - 1; index > 0; index--)
        {
            var swap = Random.Shared.Next(index + 1);
            (order[index], order[swap]) = (order[swap], order[index]);
        }

        if (count > 1 && order[0] == avoidFirst)
        {
            var swap = 1 + Random.Shared.Next(count - 1);
            (order[0], order[swap]) = (order[swap], order[0]);
        }

        return order;
    }
}
