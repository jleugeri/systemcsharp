
public class InPort<DataT> : Module, IInPort<DataT>
{
    public IEvent Updated { get; }

    public DataT? Data { get; protected set; }

    protected void OnUpdated(DataT data)
    {
        Data = data;
        Updated.Notify(0.0);
        //System.Console.WriteLine("Received data " + Data);
    }

    public InPort(string name, IEventLoop el) : base(name, el)
    {
        Updated = new Event(name + ".Updated", el);
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