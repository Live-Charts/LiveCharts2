// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Motion;

internal class CanvasZone
{
    public const int DrawMargin = 0;
    public const int NoClip = 1;
    public const int XCrosshair = 2;
    public const int YCrosshair = 3;
    // Overlay draws last so tooltips and legends are never obscured by ticks/labels
    // from the crosshair zones above (issue #2304).
    public const int Overlay = 4;
    private Paint[] _sortedPaints = [];
    private bool _isDirty = true;

    public int StateId { get; set; } = -1;
    public LvcRectangle Clip { get; set; } = LvcRectangle.Empty;
    private HashSet<Paint> PaintTasks { get; set; } = [];

    // one instance per zone
    public static CanvasZone[] CreateZones() =>
        [new CanvasZone(), new CanvasZone(), new CanvasZone(), new CanvasZone(), new CanvasZone()];

    public Paint[] EnumerateTasks()
    {
        if (!_isDirty)
            return _sortedPaints;

#if DEBUG
        if (LiveCharts.EnableLogging)
            Debug.WriteLine($"[LiveCharts] CanvasZone paint tasks sorted by ZIndex");
#endif

        _sortedPaints = [..
                PaintTasks
                    .Where(x => x is not null && x != Paint.Default)
                    .OrderBy(x => x.ZIndex)
            ];

        _isDirty = false;
        return _sortedPaints;
    }

    public void AddTask(Paint task)
    {
        if (task is null || task == Paint.Default) return;

        if (PaintTasks.Add(task))
        {
            task.ZIndexChanged += OnZIndexChanged;
            _isDirty = true;
        }
    }

    public bool RemoveTask(Paint task)
    {
        var removed = PaintTasks.Remove(task);

        if (removed)
        {
            task.ZIndexChanged -= OnZIndexChanged;
            _isDirty = true;
        }

        return removed;
    }

    public bool ContainsTask(Paint task) =>
        PaintTasks.Contains(task);

    public void ClearTasks()
    {
        foreach (var task in PaintTasks)
        {
            task.ZIndexChanged -= OnZIndexChanged;
        }

        PaintTasks.Clear();
        _isDirty = true;
    }

    public int CountTasks() =>
        PaintTasks.Count;

    private void OnZIndexChanged() =>
        _isDirty = true;
}
