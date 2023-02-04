namespace SystemCSharp;

public class InPort<DataT> : Module, IInPort<DataT> where DataT : IEquatable<DataT>
{

    protected Signal<DataT>? _data = null;
    protected DataT? nextData = default(DataT);

    public Signal<DataT> Signal
    {
        get
        {
            if(_data != null)
                return _data;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }

    public DataT Value
    {
        get
        {
            if(_data != null)
                return _data.Value;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }

    public IEvent Changed
    {
        get
        {
            if(_data != null)
                return _data.Changed;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }
    public IEvent Updated
    {
        get
        {
            if(_data != null)
                return _data.Updated;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }

    public bool WasChanged { 
        get
        {
            if (_data != null)
                return _data.WasChanged;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
        
        set
        {
            if (_data != null) _data.WasChanged = value;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }
    public bool WasUpdated { 
        get
        {
            if (_data != null)
                return _data.WasUpdated;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
        
        set
        {
            if (_data != null) _data.WasUpdated = value;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
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