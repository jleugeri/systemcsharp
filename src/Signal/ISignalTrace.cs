public interface ISignalTrace<T> : IEnumerable<(double Time, T Value)> where T : IEquatable<T>
{
    string Name { get; }
    ISignal<T>? Signal { get; }
    List<double> Times { get; }
    List<T> Values { get; }

    void Add(double time, T value);
    void Clear();
    void Trace(ISignal<T>? signal = null);
}
