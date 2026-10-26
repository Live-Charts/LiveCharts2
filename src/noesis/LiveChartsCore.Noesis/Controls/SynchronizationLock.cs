using Noesis;

namespace LiveChartsCore.Noesis;

internal sealed class SynchronizationLock : BaseComponent
{
    public SynchronizationLock() : this(new object())
    {
    }

    public SynchronizationLock(object value)
    {
        Value = value;
    }

    internal object Value { get; }
}
