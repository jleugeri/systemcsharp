namespace SystemCSharp;
public class Signal<DataT> : ISignal<DataT> where DataT : IEquatable<DataT>
{
    public string Name { get; }
    public Event Changed { get; }
    public Event Updated { get; }

    protected DataT Initial;

    protected IEventLoop EventLoop;

    public Signal(string name, DataT initial, IEventLoop eventLoop)
    {
        Name = name;
        Changed = new(name + ".Changed", eventLoop);
        Updated = new(name + ".Updated", eventLoop);
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
            Changed.Notify(0.0);

        Updated.Notify(0.0);
        _value = _nextValue;
    }

    public void Reset()
    {
        _value = Initial;
        _nextValue = Initial;
    }
}
