namespace SystemCSharp;
public interface IOutPort<DataT>
{
    string Name { get; }
    DataT Data { get; set; }
    Action<DataT>? Updated { get; set; }
}
