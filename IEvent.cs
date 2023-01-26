using System.Runtime.CompilerServices;

///<summary>
/// An IEvent can be notified with delay and awaited for simulation of asynchronous systems.
///</summary>
public interface IEvent
{
    ///<summary>
    ///Holds all the statically scheduled actions for execution upon 
    ///the next trigger of this event.
    ///</summary>
    Action? StaticSensitivity { get; set; }
    
    ///<summary>
    ///Holds all the dynamically scheduled actions for execution upon 
    ///the next trigger of this event.
    ///</summary>
    Action? DynamicSensitivity { get; set; }

    ///<summary>Name of the event.</summary>
    string Name { get; }

    IEventAwaiter GetAwaiter();

    ///<summary>
    ///Notifies the event-loop to schedule this event with `delay`
    ///</summary>
    void Notify(double delay);

    ///<summary>
    ///Notifies the event-loop to schedule this event immediately
    ///</summary>
    void Notify();
}
