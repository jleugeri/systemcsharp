class InPort<DataT>
{
    public string name { get; }
    public Event Updated { get; }

    public DataT? data { get; protected set; }

    protected void OnUpdated(DataT _data)
    {
        data = _data;
        Updated.notify(0.0);
        System.Console.WriteLine("Received data " + data);
    }

    public InPort(string _name, EventLoop el) 
    {
        name = _name;
        Updated = new Event(_name+".Updated", el);
    }

    public void bind(OutPort<DataT> source)
    {
        // subscribe to the source port's Updated event
        source.Updated += OnUpdated;
    }
}