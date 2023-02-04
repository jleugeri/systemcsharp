using System.Runtime.CompilerServices;
namespace SystemCSharp;

///<summary>
/// An IEvent can be notified (with delay) and awaited for during the simulation of an asynchronous system.
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

    ///<summary>
    ///Holds a reference to the event-loop that this event is scheduled on.
    ///</summary>
    IEventLoop EventLoop { get; }

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
