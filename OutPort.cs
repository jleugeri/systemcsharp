class OutPort<DataT>
{
    public string name { get; }

    protected DataT _data;
    public DataT data {
        get { return _data; }
        set { _data = value; OnUpdated(); }
    }

    public delegate void UpdatedEventHandler(DataT data);
    public event UpdatedEventHandler? Updated;

    protected virtual void OnUpdated()
    {
        if(Updated != null)
            Updated(data);
    }

    public OutPort(string _name, DataT initial_data) 
    {
        name = _name;
        _data = initial_data;
    }
}