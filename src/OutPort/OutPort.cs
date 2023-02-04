namespace SystemCSharp;

public class OutPort<DataT> : Module, IOutPort<DataT> where DataT: IEquatable<DataT>
{
    protected Signal<DataT> _data;
    public DataT Value { get { return _data.Value; } set { _data.Value=value; } }

    public OutPort(string name, DataT initialData, IEventLoop eventLoop) : base(name, eventLoop)
    {
        _data = new(name+".Signal", initialData, eventLoop);
    }

    public override void Reset()
    {
        _data.Reset();
    }

    public void Bind(IInPort<DataT> other)
    {
        other.Bind(_data);
    }
}