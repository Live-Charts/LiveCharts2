using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CoreTests.MockedObjects;
using LiveChartsCore.Kernel.Observers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

// The observer's per-item walk (subscribe INotifyPropertyChanged on every item) is gated
// STATICALLY by the generic caller on the element type — no reflection over the collection
// instance, AOT/trimmer safe. Value types never walk (enumeration boxes each item, so a
// subscription would attach to a temporary box the collection does not hold), sealed
// non-INPC classes never walk; collection-level notifications still redraw, and per-item
// callbacks (the chart-level Series/Axes observers) always force the walk.
[TestClass]
public class CollectionDeepObserverSkipTests
{
    private struct InpcValue : INotifyPropertyChanged
    {
        public static int Subscriptions;

        private static void IncrementSubscriptions() =>
            _ = System.Threading.Interlocked.Increment(ref Subscriptions);

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => IncrementSubscriptions();
            remove { }
        }
    }

    private sealed class SealedPlain { }
    private sealed class SealedInpc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
    }
    private class OpenPlain { }

    [TestMethod]
    public void StaticGate_DecidesByElementType()
    {
        Assert.IsFalse(CollectionDeepObserver.MayContainTrackableItems<int>(), "primitives never need tracking");
        Assert.IsFalse(CollectionDeepObserver.MayContainTrackableItems<InpcValue>(), "an INPC STRUCT subscription lands on a dead box — untrackable");
        Assert.IsFalse(CollectionDeepObserver.MayContainTrackableItems<SealedPlain>(), "a sealed non-INPC class has no INPC instances");

        Assert.IsTrue(CollectionDeepObserver.MayContainTrackableItems<SealedInpc>(), "a sealed INPC class is tracked");
        Assert.IsTrue(CollectionDeepObserver.MayContainTrackableItems<OpenPlain>(), "an open hierarchy may hold INPC-derived instances");
        Assert.IsTrue(CollectionDeepObserver.MayContainTrackableItems<object>(), "object may hold anything");
        Assert.IsTrue(CollectionDeepObserver.MayContainTrackableItems<INotifyPropertyChanged>(), "the interface itself is trackable");
    }

    [TestMethod]
    public void InpcStructItems_AreNotSubscribed()
    {
        // Subscribing a struct's PropertyChanged through the walk attaches the handler to
        // the enumeration's TEMPORARY BOX — it can never fire, and the box stays rooted
        // in the observer. The walk must be skipped for value types entirely.
        InpcValue.Subscriptions = 0;
        var collection = new ObservableCollection<InpcValue> { new(), new() };

        var observer = new TestObserver<InpcValue>
        {
            MyCollection = collection
        };

        Assert.AreEqual(0, InpcValue.Subscriptions, "a boxed-copy subscription can never fire — it must not be made");

        collection.Add(new InpcValue());
        Assert.AreEqual(1, observer.ChangesCount, "collection changes still redraw");
        Assert.AreEqual(0, InpcValue.Subscriptions, "per-change items are not subscribed either");
    }

    [TestMethod]
    public void ValueTypeItems_StillRedrawOnEveryCollectionChange()
    {
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var observer = new TestObserver<int>
        {
            MyCollection = collection
        };

        collection.Add(4);
        Assert.AreEqual(1, observer.ChangesCount, "Add redraws");

        collection.RemoveAt(0);
        Assert.AreEqual(2, observer.ChangesCount, "Remove redraws");

        collection.Clear();
        Assert.AreEqual(3, observer.ChangesCount, "Reset redraws");

        observer.Dispose(); // the skipped walk must not break the symmetric dispose
    }

    [TestMethod]
    public void PerItemCallbacks_ForceTheWalk()
    {
        // The chart-level observers (Series/Axes/Sections) rely on per-item add/remove
        // callbacks — those must keep walking even for untrackable element types.
        var seen = new List<object>();
        var observer = new CollectionDeepObserver(
            () => { }, onItemAdded: seen.Add, onItemRemoved: null, trackItemProperties: false);

        var collection = new ObservableCollection<int> { 1, 2, 3 };
        observer.Initialize(collection);
        Assert.AreEqual(3, seen.Count, "initialization raises the callback per existing item");

        collection.Add(4);
        Assert.AreEqual(4, seen.Count, "changes raise the callback per added item");

        observer.Dispose();
    }

    [TestMethod]
    public void DefaultCtorWalk_NeverSubscribesInpcStructs()
    {
        // Defense in depth: even when the walk runs with tracking ON (the default 3-arg
        // ctor — non-generic callers can't use the static gate), an INPC STRUCT must not
        // be subscribed: the handler would attach to the enumeration's temporary box and
        // root it in the observer.
        InpcValue.Subscriptions = 0;
        var observer = new CollectionDeepObserver(() => { });

        var collection = new ObservableCollection<InpcValue> { new(), new() };
        observer.Initialize(collection);
        collection.Add(new InpcValue());

        Assert.AreEqual(0, InpcValue.Subscriptions, "boxed-copy subscriptions can never fire — none may be made");

        observer.Dispose();
    }

    [TestMethod]
    public void CallbackWalk_WithTrackingOff_DoesNotSubscribeInpc()
    {
        // The walk can be forced by per-item callbacks while property tracking is off
        // (untrackable element types): the callbacks must fire per item, but no
        // PropertyChanged subscription may be made — for value types that subscription
        // would land on the enumeration's temporary box and root it.
        InpcValue.Subscriptions = 0;
        var seen = new List<object>();
        var observer = new CollectionDeepObserver(
            () => { }, onItemAdded: seen.Add, onItemRemoved: null, trackItemProperties: false);

        var collection = new ObservableCollection<InpcValue> { new(), new() };
        observer.Initialize(collection);
        Assert.AreEqual(2, seen.Count, "the callback walk still runs");
        Assert.AreEqual(0, InpcValue.Subscriptions, "no PropertyChanged subscription on a walked box");

        collection.Add(new InpcValue());
        Assert.AreEqual(3, seen.Count);
        Assert.AreEqual(0, InpcValue.Subscriptions, "per-change items are not subscribed either");

        observer.Dispose();
    }
}
