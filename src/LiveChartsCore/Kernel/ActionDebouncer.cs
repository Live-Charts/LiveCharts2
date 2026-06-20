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

using System;
using System.Diagnostics;
using System.Threading;

namespace LiveChartsCore.Kernel;

internal sealed class ActionDebouncer(TimeSpan delay) : IDisposable
{
    private readonly TimeSpan _delay = delay;
    private readonly object _gate = new();
    private Timer? _timer;
    private Action? _pending;

    public void Debounce(Action action)
    {
        lock (_gate)
        {
            _pending = action;
            if (_timer is null)
                _timer = new Timer(OnTick, null, _delay, Timeout.InfiniteTimeSpan);
            else
                _ = _timer.Change(_delay, Timeout.InfiniteTimeSpan);
        }
    }

    public void Dispose()
    {
        // Idempotent and revivable: if Debounce is called after Dispose (e.g.
        // chart Load -> Unload -> Load), the next call creates a fresh Timer.
        lock (_gate)
        {
            _timer?.Dispose();
            _timer = null;
            _pending = null;
        }
    }

    private void OnTick(object? state)
    {
        Action? action;
        lock (_gate)
        {
            action = _pending;
            _pending = null;
        }

        try
        {
            action?.Invoke();
        }
        catch (Exception ex)
        {
            // OnTick runs on a ThreadPool thread; an unhandled throw here
            // would terminate the process. Trace the exception (matching
            // the pattern in Chart.UpdateThrottlerUnlocked from #1826) so
            // listeners get a signal instead of a silent crash.
            Trace.WriteLine($"[LiveCharts] debounced action failed: {ex}");
        }
    }
}

