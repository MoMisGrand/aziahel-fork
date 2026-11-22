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
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Imlight.Setup;

/// <summary>
/// Bots > Zoom (Niduus, 2026-11-22: "Panel in the EXE, same as the other contenders in the exact same way"): Zoom, the Story
/// quest scout, gets the page every contender has: Ultimus's own page with its Overview, Issues and Logs, showing Zoom (his
/// folder logs\zoom, his settings [Zoom], empty = Ultimus's), exactly as a TCO7 or Kitharas Family member's page
/// (ManagerWindow.Tco7.cs). His second tab, "Zoom's mind", shows his own mind file: the broken quests and broken quest steps
/// he found ahead of Ultimus and the teams, and the quests he scouted (logs\zoom\zoom-mind.txt, read every few seconds).
/// </summary>
internal sealed partial class ManagerWindow {

    private const string ZOOM_TAB = "Zoom";
    private const string ZOOM_MIND_TAB = "Zoom's mind";
    private const string ZOOM_TEAM = "Zoom";

    /// <summary>Zoom's seat on the pages: his folder (logs\zoom) and his own section ([Zoom]).</summary>
    private static readonly UltSeat s_zoomSeat = new("Zoom", "Storm", "zoom", ZOOM_TEAM, ZOOM_TEAM, ZOOM_TEAM);

    // Written out: the side bar's list is made before the pages are.
    private static string[] ZoomTabNames => [ZOOM_TAB, ZOOM_MIND_TAB];

    private readonly TextBlock _zoomMindSummary = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBox _zoomMind = new() {
        IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, FontFamily = MONO, FontSize = 12.5, MinHeight = 420,
    };
    private readonly DispatcherTimer _zoomTimer = new() { Interval = TimeSpan.FromSeconds(3) };
    private string _zoomMindText = "";

    /// <summary>Zoom's page (the same as every contender's) and his mind file's page.</summary>
    private IEnumerable<TabItem> ZoomTabs() {
        yield return Tab(ZOOM_TAB, () => Tco7MemberTab(s_zoomSeat));
        yield return Tab(ZOOM_MIND_TAB, ZoomMindTab);
    }

    /// <summary>Zoom is kept out only by his own switch ([Zoom] Enabled, on unless turned off): he has no team.</summary>
    private string ZoomOffWhy()
        => string.Equals(_manager.GetSetting(ZOOM_TEAM, "Enabled")?.Trim(), "false", StringComparison.OrdinalIgnoreCase)
            ? "his switch is off (Enabled at the top of this page)" : "";

    private Control ZoomMindTab() {
        _zoomTimer.Tick += (_, _) => {
            if (_shownPage == ZOOM_MIND_TAB) {
                RefreshZoomMind();
            }
        };
        _zoomTimer.Start();
        RefreshZoomMind();

        return Page(
            Fold("flag", BOTS, "Zoom's mind",
                "Zoom, the quest scout, runs the main Story quests ahead of Ultimus and the teams to find what is broken before they " +
                "get there. He is top level with True Form's gear and spells, quests with Ultimus's brain and everything the bots and " +
                "the Hive mind know, teleports to each step and wins every fight in one hit. He never finishes anything by command. " +
                "A step that does not finish in 10 minutes is a broken step; no next main quest 10 minutes after one he finished is a " +
                "broken quest. Each is written here, in his own mind file (logs\\zoom\\zoom-mind.json), and in his Issues.",
                _zoomMindSummary,
                _zoomMind));
    }

    private void RefreshZoomMind() {
        var folder = Path.Combine(_manager.LogFolder, s_zoomSeat.Folder);
        var text = ReadAllOf(Path.Combine(folder, "zoom-mind.txt"), 512 * 1024).TrimEnd();
        if (text.Length == 0) {
            text = "Nothing yet. Zoom writes here once he finds a broken quest or step, or finishes a quest.";
        }

        var first = text.Split('\n').FirstOrDefault()?.Trim() ?? "";
        _zoomMindSummary.Text = first.StartsWith("Zoom's mind", StringComparison.Ordinal) ? first : "";
        if (_zoomMindText != text) {
            _zoomMindText = text;
            _zoomMind.Text = text;
        }
    }

}
