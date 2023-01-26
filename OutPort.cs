
public class OutPort<DataT> : IOutPort<DataT>
{
    public string Name { get; }

    protected DataT _data;
    public DataT Data
    {
        get { return _data; }
        set { _data = value; OnUpdated(); }
    }

    public Action<DataT>? Updated { get; set; }

    protected virtual void OnUpdated()
    {
        if (Updated != null)
            Updated(Data);
    }

    public OutPort(string name, DataT initialData)
    {
        Name = name;
        _data = initialData;
    }
}