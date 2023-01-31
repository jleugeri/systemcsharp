namespace SystemCSharp;
public class Signal<DataT> : ISignal<DataT> where DataT : IEquatable<DataT>
{
    public string Name { get; }
    public Event Changed { get; set; }

    public DataT Initial { get; protected set; }

    public Signal(string name, DataT initial, IEventLoop eventLoop)
    {
        Name = name;
        Changed = new(name + ".Changed", eventLoop);
        _value = initial;
        Initial = initial;
    }

    protected DataT _value;
    public DataT Value
    {
        get
        {
            return _value;
        }

        set
        {
            if (!_value.Equals(value))
            {
                _value = value;
                Changed.Notify(0.0);
            };
        }
    }

    public void Reset()
    {
        _value = Initial;
    }
}
