using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;

namespace PvpAutoLb.Core;

internal sealed class SessionStats
{
    private const double KillAttributionWindowSeconds = 5.0;

    private readonly Configuration cfg;
    private readonly List<WatchedFire> watching = new();

    public int TotalFires { get; private set; }
    public int KillsAttributed { get; private set; }
    public int EnemiesAffectedTotal { get; private set; }
    public DateTime StartedUtc { get; private set; } = DateTime.UtcNow;

    public SessionStats(Configuration cfg)
    {
        this.cfg = cfg;
    }

    public void RecordFire(IBattleChara target, int enemiesAffected)
    {
        TotalFires++;
        EnemiesAffectedTotal += enemiesAffected;
        cfg.LifetimeFires++;
        cfg.LifetimeEnemiesAffected += (uint)Math.Max(0, enemiesAffected);
        cfg.SaveDebounced();
        watching.Add(new WatchedFire(target.EntityId, DateTime.UtcNow));
    }

    public void Tick()
    {
        if (watching.Count == 0) return;

        var now = DateTime.UtcNow;
        var killCounted = false;
        for (var i = watching.Count - 1; i >= 0; i--)
        {
            var w = watching[i];
            var entity = Svc.Objects.SearchById(w.EntityId);
            var dead = entity == null || (entity is IBattleChara b && (b.IsDead || b.CurrentHp == 0));

            if (dead)
            {
                KillsAttributed++;
                cfg.LifetimeKills++;
                killCounted = true;
                watching.RemoveAt(i);
            }
            else if ((now - w.At).TotalSeconds > KillAttributionWindowSeconds)
            {
                watching.RemoveAt(i);
            }
        }
        if (killCounted) cfg.SaveDebounced();
    }

    public void ResetSession()
    {
        TotalFires = 0;
        KillsAttributed = 0;
        EnemiesAffectedTotal = 0;
        StartedUtc = DateTime.UtcNow;
        watching.Clear();
    }

    public void ResetLifetime()
    {
        cfg.LifetimeFires = 0;
        cfg.LifetimeKills = 0;
        cfg.LifetimeEnemiesAffected = 0;
        cfg.Save();
    }

    private readonly record struct WatchedFire(ulong EntityId, DateTime At);
}
