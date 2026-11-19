/*
 * Imlight
 * Copyright (C) 2025 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Imlight.CoreLib.Game.Quests;
using Imlight.CoreLib.Game.Tools;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// Where a bot lights the three storm obelisks before the Tomb of Storms lets it on into the Well of Spirits. The obelisks a
/// wizard presses stand on Storm Island in The Oasis (KT_Hub: KT_ScarabHubOblelisk 1-3, and the Oasis's own script plays
/// the sandstorm once all three are on). The Tomb of Storms' file also carries copies of them at the same spots, but those
/// stand off its ground where nobody can walk. TCO7 logs, 2026-11-19 03:28-06:00: Cyndr and Idunn stood in the Tomb of
/// Storms for hours walking toward obelisk 2 there ("no way on the map"), 50+ stuck flags, never lit one.
/// </summary>
internal static class BotObelisks {

    internal const string OASIS = "Krokotopia/KT_Hub";

    // The obelisks in The Oasis (its own file), by number.
    private static readonly IReadOnlyDictionary<int, (float X, float Y, float Z)> s_inOasis = new Dictionary<int, (float, float, float)> {
        [1] = (-3186f, -3008f, -28f),
        [2] = (-4690f, -2716f, -10f),
        [3] = (-4386f, -1226f, -13f),
    };

    internal static bool IsOasis(string zone) => string.Equals(zone, OASIS, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether the wizard still has obelisks to light and may light them (finished "Back to Balance").</summary>
    internal static bool ToLight(WizardData.Models.Player.Wizard wizard) => StormObelisks.Shut(wizard) && StormObelisks.MayLight(wizard);

    /// <summary>
    /// The obelisk to light next in The Oasis: the nearest one not lit yet, with its live mark when the area shows it.
    /// </summary>
    internal static (int Number, float X, float Y, float Z, MinimapIndex.Mark Mark, int Left, ulong Press)? Next(WizardData.Models.Player.Wizard wizard,
        float x, float y, IReadOnlyList<MinimapIndex.Mark> live, IEnumerable<ulong> interact = null) {
        var unlit = StormObelisks.Unlit(wizard).ToList();
        if (unlit.Count == 0) {
            return null;
        }

        var pick = unlit.Select(o => (o.Template, o.Number, At: s_inOasis[o.Number]))
            .OrderBy(o => Far(x, y, o.At.X, o.At.Y)).First();
        var mark = live?.Where(m => m.Object is not null
                                    && (m.Template == pick.Template
                                        || (m.Name ?? "").Contains("belisk", StringComparison.OrdinalIgnoreCase)
                                        || (m.Name ?? "").Contains("Oblelisk", StringComparison.OrdinalIgnoreCase))
                                    && Far(pick.At.X, pick.At.Y, m.Object.m_location.X, m.Object.m_location.Y) < 300)
            .OrderBy(m => Far(pick.At.X, pick.At.Y, m.Object.m_location.X, m.Object.m_location.Y)).FirstOrDefault();

        // The obelisk is a big pillar on a block nobody walks on: the walk ends at its foot, some 130-150 from its middle
        // (TCO7 logs, 2026-11-19 09:00: Cyndr, Garm and Idunn stood at obelisk 3's foot for minutes). The minimap does not
        // list it, but by its foot the game offers it to the wizard (an Interact option within 300), as to a player: that
        // offer is what gets pressed, one not tried in the last two minutes first (so a wrong pick is not pressed for ever).
        var press = 0UL;
        if (mark is null && interact is not null && Far(x, y, pick.At.X, pick.At.Y) <= PRESS_FROM) {
            var known = new HashSet<ulong>(live?.Where(m => m.Object is not null).Select(m => m.Object.m_globalID.Full) ?? []);
            press = interact.Where(id => !known.Contains(id))
                .OrderBy(id => s_tried.TryGetValue(id, out var at) && DateTime.UtcNow - at < TimeSpan.FromMinutes(2) ? 1 : 0)
                .FirstOrDefault();
        }

        return (pick.Number, pick.At.X, pick.At.Y, pick.At.Z, mark, unlit.Count, press);
    }

    // How near the obelisk's middle a wizard is once at its foot.
    internal const float PRESS_FROM = 260f;

    // The offers pressed for an obelisk, and when.
    private static readonly ConcurrentDictionary<ulong, DateTime> s_tried = new();

    /// <summary>An offer pressed as an obelisk.</summary>
    internal static void Pressed(ulong id) => s_tried[id] = DateTime.UtcNow;

    private static float Far(float x1, float y1, float x2, float y2) => MathF.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
}
