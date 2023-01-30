public interface ISignal<DataT>  where DataT : IEquatable<DataT>
{
    string Name { get; }
    Event Changed { get; set; }
    DataT Value { get; set; }
    DataT Initial { get; }

    void Reset();
}
