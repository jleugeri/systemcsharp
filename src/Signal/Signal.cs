namespace SystemCSharp;
using Serilog;
public class Signal<DataT> : ISignal<DataT> where DataT : IEquatable<DataT>
{
    public string Name { get; }
    public Event Changed { get; }
    public Event Updated { get; }

    public bool WasChanged { get; set; }
    public bool WasUpdated { get; set; }

    protected DataT Initial;

    protected IEventLoop EventLoop;

    public Signal(string name, DataT initial, IEventLoop eventLoop)
    {
        Name = name;
        Changed = new(name + ".Changed", eventLoop);
        Updated = new(name + ".Updated", eventLoop);
        WasChanged = false;
        WasUpdated = false;
        _value = initial;
        _nextValue = initial;
        Initial = initial;
        EventLoop = eventLoop;
    }

    protected DataT _value;
    protected DataT _nextValue;

    public DataT Value
    {
        get
        {
            return _value;
        }

        set
        {
            _nextValue = value;
            EventLoop.RequestUpdate(this);
        }
    }

    public void ApplyUpdate()
    {

        if (!_value.Equals(_nextValue))
        {
            WasChanged = true;
            Changed.Notify(0.0);
        }

        WasUpdated = true;
        Updated.Notify(0.0);
        Log.Logger.Verbose("Signal '{name}': Setting value from {oldValue} to {value} (Updated: {updated} Changed: {changed}) at time {time}.", Name, _value, _nextValue, WasUpdated, WasChanged, EventLoop.SimulationTime);
        _value = _nextValue;
    }

    public void Reset()
    {
        _value = Initial;
        _nextValue = Initial;
    }
}
