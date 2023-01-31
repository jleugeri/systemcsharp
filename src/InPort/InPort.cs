namespace SystemCSharp;

public class InPort<DataT> : Module, IInPort<DataT>, IUpdate
{
    public IEvent Updated { get; }

    public DataT? Data { get; protected set; }
    protected DataT? nextData;

    public InPort(string name, IEventLoop el) : base(name, el)
    {
        Updated = new Event(name + ".Updated", el);
    }

    public void ApplyUpdate()
    {
        if(nextData != null)
            Data = nextData;
    }

    protected void OnUpdated(DataT data)
    {
        nextData = data;
        EventLoop.RequestUpdate(this);
        Updated.Notify(0.0);
        //System.Console.WriteLine("Received data " + Data);
    }

    public void Bind(IOutPort<DataT> source)
    {
        // subscribe to the source port's Updated event
        source.Updated += OnUpdated;
    }

    public void Unbind(IOutPort<DataT> source)
    {
        // subscribe to the source port's Updated event
        source.Updated -= OnUpdated;
    }

    public override void Reset()
    {
        Data = default(DataT);
    }
}