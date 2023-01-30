
public class OutPort<DataT> : Module, IOutPort<DataT>
{
    protected DataT _initialData;
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

    public OutPort(string name, DataT initialData, IEventLoop eventLoop) : base(name, eventLoop)
    {
        _initialData = initialData;
        _data = _initialData;
    }

    public override void Reset()
    {
        _data = _initialData;
    }
}