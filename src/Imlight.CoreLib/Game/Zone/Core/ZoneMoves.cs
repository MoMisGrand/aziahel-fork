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
 *
 * ========================================================================
 * ZONE MOVES
 * ========================================================================
 *
 * PURPOSE:
 * Moves of a zone's OWN objects and NPCs (the ones in its gamedata.bin), made in Blood Moon, Niduus's 3D zone viewer
 * (Niduus, 2026-11-22: "moving certain NPC's update to Ahziael in real time"). Read from
 * spiraldb-extra/Customs/ZoneMoves.json, which Blood Moon writes; the game files are never changed. The file:
 *   { "WizardCity/WC_Hub": { "88:77892": { "X": .., "Y": .., "Z": .., "Yaw": .., "Name": "..", "TemplateID": 77892 } } }
 * A key is the object's place in the zone's own object list (gamedata.bin m_objectList, empty entries counted) and its
 * template id; the template must match, so a game update that reorders the list moves nothing by mistake. Yaw is in
 * radians. Name, TemplateID and Saved are notes for Blood Moon.
 * Every copy of the zone uses the moves when it loads (ZoneObjects.AddTo marks the list before anything is added or left
 * out), and the file is watched: a change reaches every loaded copy at once (MSG_ZONEOBJECTSCHANGED OwnMovesChanged), where
 * ZoneObjectSupervisor takes a moved object away from the players and sends it again at its new place. An object whose
 * move is taken out of the file goes back where the game has it.
 * Battle circles and dungeon sigil pads are left where they are (their own supervisor places them).
 *
 * Created by: Claude
 * Version: KALI 1.0
 * Last Updated: 11/22/2026
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Imcodec.Math;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData;

namespace Imlight.CoreLib.Game.Zone.Core;

internal static class ZoneMoves {

    internal const string FILE = "ZoneMoves.json";
    private const string FOLDER = "Customs";
    private const int RELOAD_DELAY_IN_MS = 400;

    internal sealed class Move {
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Z { get; set; }
        public float? Yaw { get; set; }
    }

    // A zone's own object as the game placed it, and its key in the file.
    private sealed record Own(string Zone, string Key, Vector3 Location, Vector3 Orientation);

    private static readonly ConditionalWeakTable<CoreObjectInfo, Own> s_own = [];
    private static readonly object s_lock = new();
    private static Dictionary<string, Dictionary<string, Move>> s_moves;
    private static FileSystemWatcher s_watcher;
    private static Timer s_reloadTimer;

    private static readonly JsonSerializerOptions s_options = new() {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Notes each of a loading zone's own objects with its key and where the game placed it, and moves the ones the file
    /// moves. Called with the zone's object list as the game files have it, before anything is added or left out.
    /// </summary>
    internal static void Mark(string zonePath, List<CoreObjectInfo> objects) {
        if (string.IsNullOrEmpty(zonePath) || objects is null) {
            return;
        }

        var moved = 0;
        for (var i = 0; i < objects.Count; i++) {
            var info = objects[i];
            if (info is null || info is MinigameSigilInfo || info is CombatSigilObjectInfo) {
                continue;
            }

            s_own.AddOrUpdate(info, new Own(zonePath, $"{i}:{info.m_templateID.Full}", info.m_location, info.m_orientation));
            if (Apply(info)) {
                moved++;
            }
        }

        if (moved > 0) {
            Logger.Information("Zone {0}: {1} of its own objects moved by {2}.", Logger.Args(zonePath, moved, FILE));
        }
    }

    /// <summary>
    /// Puts one of a zone's own objects where the file says (or back where the game has it); true when it changed place.
    /// </summary>
    internal static bool Apply(CoreObjectInfo info) {
        if (info is null || !s_own.TryGetValue(info, out var own)) {
            return false;
        }

        var location = own.Location;
        var orientation = own.Orientation;
        if (MovesFor(own.Zone) is { } moves && moves.TryGetValue(own.Key, out var move) && move is not null) {
            location = new Vector3(move.X ?? location.X, move.Y ?? location.Y, move.Z ?? location.Z);
            orientation = new Vector3(orientation.X, orientation.Y, move.Yaw ?? orientation.Z);
        }

        if (location.Equals(info.m_location) && orientation.Equals(info.m_orientation)) {
            return false;
        }

        info.m_location = location;
        info.m_orientation = orientation;

        return true;
    }

    private static Dictionary<string, Move> MovesFor(string zonePath) {
        var all = s_moves;
        if (all is null) {
            lock (s_lock) {
                if (s_moves is null) {
                    s_moves = Load();
                    Watch();
                }

                all = s_moves;
            }
        }

        return all.TryGetValue(zonePath, out var moves) ? moves : null;
    }

    private static string FilePath => Path.Combine(SpiralDB.ExtraPath, FOLDER, FILE);

    private static Dictionary<string, Dictionary<string, Move>> Load() {
        var result = new Dictionary<string, Dictionary<string, Move>>(StringComparer.OrdinalIgnoreCase);
        try {
            if (!File.Exists(FilePath)) {
                return result;
            }

            var read = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Move>>>(File.ReadAllText(FilePath), s_options);
            foreach (var (zone, moves) in read ?? []) {
                if (!string.IsNullOrWhiteSpace(zone) && moves is not null) {
                    result[zone] = new Dictionary<string, Move>(moves, StringComparer.OrdinalIgnoreCase);
                }
            }
        }
        catch (Exception e) {
            Logger.Warning("Zone moves: {0} could not be read: {1}", Logger.Args(FILE, e.Message));
        }

        return result;
    }

    private static void Watch() {
        try {
            var dir = Path.Combine(SpiralDB.ExtraPath, FOLDER);
            Directory.CreateDirectory(dir);
            s_reloadTimer = new Timer(_ => Reload(), null, Timeout.Infinite, Timeout.Infinite);
            s_watcher = new FileSystemWatcher(dir, FILE) {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            };
            s_watcher.Changed += (_, _) => ReloadSoon();
            s_watcher.Created += (_, _) => ReloadSoon();
            s_watcher.Deleted += (_, _) => ReloadSoon();
            s_watcher.Renamed += (_, _) => ReloadSoon();
            s_watcher.EnableRaisingEvents = true;
        }
        catch (Exception e) {
            Logger.Warning("Zone moves: changes to {0} will need the zone to load again ({1}).", Logger.Args(FILE, e.Message));
        }
    }

    private static void ReloadSoon() => s_reloadTimer?.Change(RELOAD_DELAY_IN_MS, Timeout.Infinite);

    // A timer thread runs it, where an exception would end the server.
    private static void Reload() {
        try {
            var moves = Load();
            lock (s_lock) {
                s_moves = moves;
            }

            Logger.Information("Zone moves: {0} read again ({1} zones); every loaded zone moves its objects now.",
                Logger.Args(FILE, moves.Count));
            ZoneObjects.TellEveryZone(new ZONE_102_PROTOCOL.MSG_ZONEOBJECTSCHANGED { OwnMovesChanged = true });
        }
        catch (Exception e) {
            Logger.Warning("Zone moves could not be read again: {0}", Logger.Args(e.Message));
        }
    }

}
