using System.Collections.Generic;

namespace PvpAutoLb.Core;

internal static class LbClassifier
{
    // Job ids whose PvP LB is defensive/support — never auto-fired.
    private static readonly HashSet<uint> SupportJobs = new()
    {
        19, // PLD — Phalanx
        21, // WAR — Primal Scream
        23, // BRD — Final Fantasia
        25, // BLM — Soul Resonance
        33, // AST — Celestial River
        38, // DNC — Contradance
        39, // RPR — Tenebrae Lemurum
        40, // SGE — Mesotes
        42, // PCT — Advent of Chocobastion
    };

    public static LbKind Classify(uint classJobId)
        => SupportJobs.Contains(classJobId) ? LbKind.Support : LbKind.Offensive;
}
