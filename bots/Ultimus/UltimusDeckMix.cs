/*
 * Imlight
 * Copyright (C) 2026 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Imlight.CoreLib.Game.BotShared;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// How a solo wizard splits its cards between hits and support, learned from its own fights (Niduus, 2026-11-21: "Ultimus is
/// to train spells and to learn how combat works in wiz to utilize his solo aspects. Have him weed out his deck so he's got
/// primarily hits in main with support along with support in TC's. Have him learn, that is the whole point of him").
/// <list type="bullet">
/// <item>MainHits: the share of his main deck that is hits (BotCards.SoloShares), 50% to 75%, starting at 60%. A fight he
/// lost because it ran long (he ran out of hits before it fell) moves it up; a fight a big hit ended moves it down a little,
/// for one more heal or shield.</item>
/// <item>TcSupport: the share of his treasure cards that is support (shields, heals, blades, traps, stops; CardPlan.cs),
/// 50% to 90%, starting at 70%. A loss or a close call moves it up; a quick, clean win moves it down, so more treasure
/// card hits come along when support was not needed.</item>
/// </list>
/// Each move is small, stays inside those bounds (only Niduus changes them), and is said in his thoughts with its reason;
/// deck-mix.txt in his folder shows where it stands and the last moves. Kept per seat (Ultimus and each TCO7 member).
/// </summary>
internal static class UltimusDeckMix {

    internal const double TC_SUPPORT = 0.7, TC_SUPPORT_MIN = 0.5, TC_SUPPORT_MAX = 0.9;
    private const string DATA = "deck-mix.json", REPORT = "deck-mix.txt";
    private const int MOVES_KEPT = 30;
    // A fight past this long ran him out of good cards (the same line LearnFromLoss uses).
    internal const double LONG_FIGHT = 300;
    // The creature's biggest hit took this much of his health or more: a big hit ended it.
    internal const double BIG_HIT = 0.5;

    internal sealed class Mix {
        public double MainHits { get; set; } = BotCards.SOLO_HITS;
        public double TcSupport { get; set; } = TC_SUPPORT;
        public int Fights { get; set; }
        public int Lost { get; set; }
        public List<string> Moves { get; set; } = [];
    }

    private static readonly object s_lock = new();
    private static readonly SeatLocal<Mix> s_mixSeat = new();
    private static Mix s_mix { get => s_mixSeat.Value; set => s_mixSeat.Value = value; }

    /// <summary>The share of his main deck that goes to hits now.</summary>
    internal static double MainHits {
        get {
            lock (s_lock) {
                return Math.Clamp((s_mix ??= Read()).MainHits, BotCards.SOLO_HITS_MIN, BotCards.SOLO_HITS_MAX);
            }
        }
    }

    /// <summary>The share of his treasure cards that goes to support now.</summary>
    internal static double TcSupport {
        get {
            lock (s_lock) {
                return Math.Clamp((s_mix ??= Read()).TcSupport, TC_SUPPORT_MIN, TC_SUPPORT_MAX);
            }
        }
    }

    /// <summary>
    /// The lesson of one of his picked fights for his card mix: what moved and why, or null when it moved nothing.
    /// </summary>
    /// <param name="won">He won it.</param>
    /// <param name="rounds">How many rounds it ran.</param>
    /// <param name="healthLost">The share of his health it took (0 to 1).</param>
    /// <param name="seconds">How long it ran.</param>
    /// <param name="biggestHit">The creature's biggest hit as a share of his health (BotKnowledge), 0 when not known.</param>
    internal static string Learn(string foe, bool won, int rounds, double healthLost, double seconds, double biggestHit) {
        lock (s_lock) {
            var mix = s_mix ??= Read();
            mix.Fights++;
            mix.Lost += won ? 0 : 1;
            var (newHits, newSupport, why) = Step(foe, won, rounds, healthLost, seconds, biggestHit, mix.MainHits, mix.TcSupport);
            if (why is null || (Math.Abs(newHits - mix.MainHits) < 0.001 && Math.Abs(newSupport - mix.TcSupport) < 0.001)) {
                Write(mix);
                return null;
            }

            var said = $"{why}. Main deck hits {mix.MainHits:P0} -> {newHits:P0}, treasure card support {mix.TcSupport:P0} -> {newSupport:P0}.";
            mix.MainHits = newHits;
            mix.TcSupport = newSupport;
            mix.Moves.Add($"{DateTime.Now:yyyy-MM-dd HH:mm} {said}");
            if (mix.Moves.Count > MOVES_KEPT) {
                mix.Moves.RemoveRange(0, mix.Moves.Count - MOVES_KEPT);
            }

            Write(mix);
            return said;
        }
    }

    /// <summary>
    /// One fight's move of the mix (no files): the new main deck hit share, the new treasure card support share, and why
    /// (null when the fight teaches nothing about the mix). Always inside the bounds.
    /// </summary>
    internal static (double MainHits, double TcSupport, string Why) Step(string foe, bool won, int rounds, double healthLost,
                                                                          double seconds, double biggestHit, double mainHits, double tcSupport) {
        double hits = 0, support = 0;
        string why;
        if (!won && seconds >= LONG_FIGHT) {
            hits = 0.03;
            support = 0.02;
            why = $"{foe} outlasted me ({seconds / 60:0} minutes): I ran out of hits before it fell";
        }
        else if (!won && biggestHit >= BIG_HIT) {
            hits = -0.02;
            support = 0.05;
            why = $"{foe}'s big hit ({biggestHit:P0} of my health) ended it: I needed a shield or heal in hand";
        }
        else if (!won) {
            support = 0.03;
            why = $"{foe} beat me: more support to stay up longer";
        }
        else if (healthLost >= 0.6) {
            support = 0.02;
            why = $"I beat {foe}, but it took {healthLost:P0} of my health";
        }
        else if (rounds > 0 && rounds <= 5 && healthLost <= 0.25) {
            support = -0.01;
            why = $"I beat {foe} in {rounds} rounds with {healthLost:P0} of my health lost: less support was needed";
        }
        else {
            why = null;
        }

        return (Math.Round(Math.Clamp(mainHits + hits, BotCards.SOLO_HITS_MIN, BotCards.SOLO_HITS_MAX), 3),
            Math.Round(Math.Clamp(tcSupport + support, TC_SUPPORT_MIN, TC_SUPPORT_MAX), 3), why);
    }

    /// <summary>How many of <paramref name="room"/> treasure card slots go to support, the rest to hits and damage enchantments.</summary>
    internal static int SupportSlots(int room, double tcSupport)
        => room <= 0 ? 0 : Math.Clamp((int) Math.Round(room * Math.Clamp(tcSupport, TC_SUPPORT_MIN, TC_SUPPORT_MAX)), 1, room);

    private static Mix Read() {
        try {
            var path = Path.Combine(UltimusWire.Folder, DATA);
            if (File.Exists(path) && UltimusWire.Read<Mix>(File.ReadAllText(path)) is { } read) {
                read.Moves ??= [];
                return read;
            }
        }
        catch (Exception) {
            // A broken file starts the mix again from the starting shares.
        }

        return new Mix();
    }

    private static void Write(Mix mix) {
        try {
            Directory.CreateDirectory(UltimusWire.Folder);
            File.WriteAllText(Path.Combine(UltimusWire.Folder, DATA), UltimusWire.Write(mix));
            var text = new StringBuilder();
            text.AppendLine($"HOW HE SPLITS HIS CARDS, FROM HIS OWN FIGHTS (written {DateTime.Now:yyyy-MM-dd HH:mm})");
            text.AppendLine($"Main deck: {mix.MainHits:P0} hits (starts at {BotCards.SOLO_HITS:P0}, kept between {BotCards.SOLO_HITS_MIN:P0} and {BotCards.SOLO_HITS_MAX:P0}); the rest blades, traps, heals and shields.");
            text.AppendLine($"Treasure cards: {mix.TcSupport:P0} support (starts at {TC_SUPPORT:P0}, kept between {TC_SUPPORT_MIN:P0} and {TC_SUPPORT_MAX:P0}); the rest hits and damage enchantments.");
            text.AppendLine($"Fights counted: {mix.Fights}, lost: {mix.Lost}.");
            text.AppendLine("Last moves:");
            foreach (var move in Enumerable.Reverse(mix.Moves)) {
                text.AppendLine("  " + move);
            }

            File.WriteAllText(Path.Combine(UltimusWire.Folder, REPORT), text.ToString());
        }
        catch (IOException) {
        }
        catch (UnauthorizedAccessException) {
        }
    }

}
