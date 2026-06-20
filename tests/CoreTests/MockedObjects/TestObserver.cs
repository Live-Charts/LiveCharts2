using System;
using System.Collections.Generic;
using LiveChartsCore.Kernel.Observers;

namespace CoreTests.MockedObjects;

public class TestObserver<T> : IDisposable
{
    private readonly CollectionDeepObserver _observerer;

    public TestObserver()
    {
        // Mirrors Series<TModel>: the per-item INPC walk is gated statically on the
        // element type (value types and sealed non-INPC classes never walk).
        _observerer = new CollectionDeepObserver(
            () => ChangesCount++, null, null, CollectionDeepObserver.MayContainTrackableItems<T>());
    }

    public IEnumerable<T> MyCollection
    {
        get;
        set
        {
            _observerer.Dispose();
            _observerer.Initialize(value);
            field = value;
        }
    }

    public int ChangesCount { get; private set; }

    public void Dispose() => _observerer.Dispose();
}
