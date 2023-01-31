namespace SystemCSharp;

///<summary>
///An IEventLoop can schedule IEvents
///</summary>
public interface IEventLoop
{
    double SimulationTime { get; }
    int Count { get; }

    void Notify(IEvent ev, double dt);
    void Notify(IEvent ev);
    void Run();
    void RequestUpdate(IUpdate obj);

    void Reset();
}
