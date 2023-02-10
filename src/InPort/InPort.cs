namespace SystemCSharp;

public class InPort<DataT> : Module, IInPort<DataT> where DataT : IEquatable<DataT>
{

    protected DataT? nextData = default(DataT);

    public Signal<DataT>? Signal { get; protected set; }

    public DataT Value
    {
        get
        {
            if(Signal != null)
                return Signal.Value;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }

    public IEvent Changed
    {
        get
        {
            if(Signal != null)
                return Signal.Changed;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }
    public IEvent Updated
    {
        get
        {
            if(Signal != null)
                return Signal.Updated;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }

    public bool WasChanged { 
        get
        {
            if (Signal != null)
                return Signal.WasChanged;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
        
        set
        {
            if (Signal != null) Signal.WasChanged = value;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
    }
    public bool WasUpdated { 
        get
        {
            if (Signal != null)
                return Signal.WasUpdated;
            else throw new InvalidOperationException("Cannot access port before it has been bound.");
        }
        
        set
        {
            if (Signal != null) Signal.WasUpdated = value;
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
        Signal = source;
    }

    public void Unbind()
    {
        Signal = null;
    }


    public override void Reset()
    {
        Unbind();
    }
}