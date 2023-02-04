namespace SystemCSharp;

public class InPort<DataT> : Module, IInPort<DataT> where DataT : IEquatable<DataT>
{

    protected Signal<DataT>? _data = null;
    protected DataT? nextData = default(DataT);

    public DataT Value
    {
        get
        {
            return (_data != null) ? _data.Value :
            throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }
    public IEvent Changed
    {
        get
        {
            return (_data != null) ? _data.Changed :
            throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }
    public IEvent Updated
    {
        get
        {
            return (_data != null) ? _data.Updated :
            throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }


    public InPort(string name, IEventLoop el) : base(name, el)
    {
    }

    public void Bind(IOutPort<DataT> source)
    {
        source.Bind(this);
    }

    public void Bind(Signal<DataT> source)
    {
        _data = source;
    }

    public void Unbind()
    {
        _data = null;
    }


    public override void Reset()
    {
        Unbind();
    }
}