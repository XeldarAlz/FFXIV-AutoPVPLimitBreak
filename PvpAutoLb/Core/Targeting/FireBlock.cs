namespace PvpAutoLb.Core;

internal enum FireBlock : byte
{
    None,
    AboveThreshold,
    Doomed,
    Guarded,
    Invulnerable,
    Blocklisted,
    OutOfRange,
}
