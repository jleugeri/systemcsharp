namespace SystemCSharp;

///<summary>
///An IEventLoop can schedule IEvents
///</summary>
public interface IEventLoop
{
    ///<summary>
    ///Returns the current simulation time
    ///</summary>
    double SimulationTime { get; }

    ///<summary>
    ///Returns the number of events currently queued in the event loop
    ///</summary>
    int Count { get; }

    ///<summary>
    ///Schedules an event `ev` to be notified with delay `dt`
    ///</summary>
    void Notify(IEvent ev, double dt);

    ///<summary>
    ///Schedules an event `ev` to be triggered immediately (i.e. in the current delta-cycle)
    ///</summary>
    void Notify(IEvent ev);
    
    ///<summary>
    ///Runs the event loop until completion.
    ///</summary>
    void Run();
    
    ///<summary>
    ///Schedules an update for `obj`
    ///</summary>
    void RequestUpdate(IUpdate obj);

    void Reset();
}
