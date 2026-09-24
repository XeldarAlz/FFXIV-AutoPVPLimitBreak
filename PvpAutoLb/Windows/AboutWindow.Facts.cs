using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows;

public sealed partial class AboutWindow
{
    private readonly record struct FactCategory(FontAwesomeIcon Icon, string Header, Vector4 Color, string[] Lines);

    private const float FactTooltipWidth = 320f;

    private static readonly FactCategory[] Categories =
    [
        new(FontAwesomeIcon.Heart, "A little reminder", Styling.AccentRose,
        [
            "Been at it a while? Roll your shoulders and take one slow breath.",
            "Hydration check. When did you last drink some water?",
            "Blink a few times and let your eyes rest for a moment.",
            "Stand up, stretch, and shake out your hands. Future you says thanks.",
            "Sit up and settle in comfortably. Your back will thank you later.",
            "Remember to eat something today. You matter more than any score.",
            "Eyes feel tired? Look at something far away for twenty seconds.",
            "Whatever you're chasing, you're allowed to take a break whenever.",
            "You're doing great. Be a little kinder to yourself today.",
            "A glass of water and a quick stretch can reset a long session.",
            "Unclench your jaw and drop your shoulders. There you go.",
            "Rest is part of the journey too. Step away whenever you need to.",
        ]),
        new(FontAwesomeIcon.Lightbulb, "Did you know?", Styling.AccentAmberSoft,
        [
            "Honey never spoils. Jars over 3,000 years old have been found still edible.",
            "Octopuses have three hearts and blue blood.",
            "A day on Venus is longer than a whole year on Venus.",
            "Bananas are berries, but strawberries aren't.",
            "There are more possible chess games than atoms in the observable universe.",
            "Sharks have been around longer than trees have.",
            "A group of flamingos is called a flamboyance.",
            "Honeybees can recognize individual human faces.",
            "Wombat droppings are cube shaped.",
            "The Eiffel Tower can grow over 15 cm taller on a hot day.",
            "Hot water can sometimes freeze faster than cold water.",
            "A bolt of lightning is roughly five times hotter than the surface of the Sun.",
        ]),
        new(FontAwesomeIcon.Star, "Words to live by", Styling.AccentMintSoft,
        [
            "Done is better than perfect. You can always polish later.",
            "Small steps every day add up to surprising distances.",
            "Comparison is the thief of joy. Run your own race.",
            "Progress, not perfection.",
            "You don't have to be great to start, but you have to start to be great.",
            "Be patient with yourself. Growth takes time.",
            "The best time to begin was yesterday. The second best is right now.",
            "Celebrate the small wins. They count too.",
            "Slow progress is still progress.",
            "Your only real competition is who you were yesterday.",
        ]),
        new(FontAwesomeIcon.GrinBeam, "Just for fun", Styling.AccentBlueSoft,
        [
            "Why don't scientists trust atoms? Because they make up everything.",
            "I would tell you a chemistry joke, but I know I wouldn't get a reaction.",
            "Why did the scarecrow win an award? He was outstanding in his field.",
            "I'm reading a book about anti-gravity. It's impossible to put down.",
            "Why don't skeletons fight each other? They don't have the guts.",
            "What do you call fake spaghetti? An impasta.",
            "Why did the bicycle fall over? It was two tired.",
            "What do you call cheese that isn't yours? Nacho cheese.",
            "I'm on a seafood diet. I see food, and I eat it.",
            "I only know 25 letters of the alphabet. I don't know y.",
        ]),
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
        if (!ImGui.IsMouseHoveringRect(min, max))
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
        using (ImRaii.Tooltip())
        {
            ImGui.PushTextWrapPos(FactTooltipWidth * scale);
            using (ImRaii.PushFont(UiBuilder.IconFont))
            using (ImRaii.PushColor(ImGuiCol.Text, category.Color))
            {
                ImGui.TextUnformatted(category.Icon.ToIconString());
            }

            ImGui.SameLine(0, 6f * scale);
            using (ImRaii.PushColor(ImGuiCol.Text, category.Color))
            {
                ImGui.TextUnformatted(category.Header);
            }

            ImGui.Spacing();
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            {
                ImGui.TextUnformatted(category.Lines[factLine]);
            }

            ImGui.PopTextWrapPos();
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
