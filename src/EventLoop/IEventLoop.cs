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
    ///Event that is triggered once the event-loop starts.
    ///This is inteded to be used e.g. in asynchronous tasks that should only
    ///start running after everything has been set up and the event loop started.
    ///</summary>
    IEvent Started { get; }

    ///<summary>
    ///Event that is triggered once the event-loop has processed all pending events.
    ///Since other events can be triggered by code waiting for this event,
    ///the execution of the event loop may continue afterwards.
    ///This event is intended to be used by code that should run in simulation time,
    ///but only after the "main" simulation has completed - e.g. logging the end of simulation.
    ///</summary>
    IEvent Completed { get; }

    ///<summary>
    ///Cancellation token that signals when the event loop and all scheduled tasks
    ///should be canceled.
    ///</summary>
    CancellationToken CancellationToken { get; }

    ///<summary>
    ///Returns the number of events currently queued in the event loop
    ///</summary>
    int Count { get; }

    ///<summary>
    ///Associate an event with this event queue
    ///</summary>
    void RegisterEvent(IEvent ev);

    ///<summary>
    ///Disassociate an event with this event queue
    ///</summary>
    void UnregisterEvent(IEvent ev);

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
    void Run(double maximumDuration = double.PositiveInfinity);
    
    ///<summary>
    ///Schedules an update for `obj`
    ///</summary>
    void RequestUpdate(IUpdate obj);

    void Reset();
}
