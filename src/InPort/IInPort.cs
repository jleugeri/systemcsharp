namespace SystemCSharp;
public interface IInPort<DataT>
{
    string Name { get; }
    IEvent Updated { get; }
    DataT? Data { get; }

    void Bind(IOutPort<DataT> source);
}
