class OutPort<DT>
{
    public string Name { get; }

    protected DT Data;
    public DT data {
        get { return Data; }
        set { Data = value; OnUpdated(); }
    }

    public delegate void UpdatedEventHandler(DT data);
    public event UpdatedEventHandler? Updated;

    protected virtual void OnUpdated()
    {
        if(Updated != null)
            Updated(data);
    }

    public OutPort(string name, DT initialData) 
    {
        Name = name;
        Data = initialData;
    }
}