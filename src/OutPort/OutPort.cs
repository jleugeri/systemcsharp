namespace SystemCSharp;

public class OutPort<DataT> : Module, IOutPort<DataT> where DataT : IEquatable<DataT>
{
    public DataT Value { get { return Signal.Value; } set { Signal.Value = value; } }
    public Signal<DataT> Signal { get; }
    public OutPort(string name, DataT initialData, IEventLoop eventLoop) : base(name, eventLoop)
    {
        Signal = new(name + ".Signal", initialData, eventLoop);
    }

    public override void Reset()
    {
        Signal.Reset();
    }

    public void Bind(IInPort<DataT> other)
    {
        other.Bind(Signal);
    }
}