class InPort<DataT>
{
    public string Name { get; }
    public Event Updated { get; }

    public DataT? Data { get; protected set; }

    protected void OnUpdated(DataT data)
    {
        Data = data;
        Updated.Notify(0.0);
        System.Console.WriteLine("Received data " + Data);
    }

    public InPort(string name, EventLoop el) 
    {
        Name = name;
        Updated = new Event(name+".Updated", el);
    }

    public void Bind(OutPort<DataT> source)
    {
        // subscribe to the source port's Updated event
        source.Updated += OnUpdated;
    }
}