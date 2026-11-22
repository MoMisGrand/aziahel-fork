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
using System.Text.Json;
using Imlight.Common;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// Zoom's book of what he scouted (logs/zoom): every quest step he could not finish (broken-quests.txt, newest first, and
/// broken-quests.json), and every main quest he got through (scouted.txt), so Niduus and the bots' fixers see the broken
/// quests before Ultimus and the teams reach them.
/// </summary>
internal static class ZoomBook {

    internal const string BROKEN_TXT = "broken-quests.txt";
    internal const string BROKEN_JSON = "broken-quests.json";
    internal const string SCOUTED_TXT = "scouted.txt";

    /// <summary>One step Zoom could not finish.</summary>
    internal sealed record Broken(DateTime At, string Quest, string Title, int Step, string Goal, string Zone, string Why, string Doing, bool PushedPast);

    private static readonly object s_lock = new();
    private static List<Broken> s_broken;

    private static string Folder => UltimusSeat.Zoom.Folder;

    private static List<Broken> Load() {
        if (s_broken is not null) {
            return s_broken;
        }

        try {
            var path = Path.Combine(Folder, BROKEN_JSON);
            s_broken = File.Exists(path) ? JsonSerializer.Deserialize<List<Broken>>(File.ReadAllText(path)) ?? [] : [];
        }
        catch (Exception e) {
            Logger.Warning("Zoom: his broken quest list could not be read ({0}); a new one is started.", Logger.Args(e.Message));
            s_broken = [];
        }

        return s_broken;
    }

    /// <summary>Every broken step he found, oldest first.</summary>
    internal static List<Broken> All() {
        lock (s_lock) {
            return [.. Load()];
        }
    }

    /// <summary>Whether this step of this quest is already written up.</summary>
    internal static bool Has(string quest, string goal) {
        lock (s_lock) {
            return Load().Any(b => b.Quest == quest && b.Goal == goal);
        }
    }

    /// <summary>Writes a broken step down (once per quest and step).</summary>
    internal static void Add(Broken broken) {
        lock (s_lock) {
            var list = Load();
            if (list.Any(b => b.Quest == broken.Quest && b.Goal == broken.Goal)) {
                return;
            }

            list.Add(broken);
            try {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Path.Combine(Folder, BROKEN_JSON), JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
                var lines = new List<string> {
                    $"Broken quest steps Zoom found ahead of Ultimus and the teams: {list.Count}. Newest first.",
                    "",
                };
                foreach (var b in Enumerable.Reverse(list)) {
                    lines.Add($"{b.At.ToLocalTime():yyyy-MM-dd HH:mm}  {b.Title} ({b.Quest}), step {b.Step}: {b.Goal}");
                    lines.Add($"    Where: {b.Zone}");
                    lines.Add($"    What happened: {b.Why}");
                    lines.Add($"    He was: {b.Doing}");
                    lines.Add(b.PushedPast ? "    He finished the step by command to keep scouting." : "    He waits at it.");
                    lines.Add("");
                }

                File.WriteAllLines(Path.Combine(Folder, BROKEN_TXT), lines);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
                Logger.Warning("Zoom: could not write his broken quest list: {0}", Logger.Args(e.Message));
            }
        }
    }

    /// <summary>A main quest he got through: how long it took, how many teleports, and whether a step of it was broken.</summary>
    internal static void Scouted(string quest, string title, TimeSpan took, int hops, int broken) {
        try {
            Directory.CreateDirectory(Folder);
            File.AppendAllLines(Path.Combine(Folder, SCOUTED_TXT), [
                $"{DateTime.Now:yyyy-MM-dd HH:mm}  {title} ({quest}): {(int) took.TotalMinutes} min {took.Seconds} s, {hops} teleport{(hops == 1 ? "" : "s")}" +
                (broken > 0 ? $", {broken} broken step{(broken == 1 ? "" : "s")} pushed past" : ", clean"),
            ]);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
            Logger.Debug("Zoom: could not write his scouted list: {0}", Logger.Args(e.Message));
        }
    }

}
