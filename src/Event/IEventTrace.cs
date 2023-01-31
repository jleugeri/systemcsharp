namespace SystemCSharp;
public interface IEventTrace : IEnumerable<double>
{
    string Name { get; }
    IEvent? Event { get; }
    List<double> Times { get; }

    void Add(double time);
    void Clear();
    void Trace(IEvent? _event = null);
}
