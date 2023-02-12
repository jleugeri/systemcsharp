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
    event Action StaticSensitivity;

    ///<summary>
    ///Holds all the dynamically scheduled actions for execution upon 
    ///the next trigger of this event.
    ///</summary>
    event Action DynamicSensitivity;

    ///<summary>
    ///Returns all (dynamic, static or both) subscribers to this event.
    ///If clear is true, clears the dynamic subscribers afterwards.
    ///</summary>
    Action? GetSubscribers(bool dynamicSensitivity=true, bool staticSensitivity=true, bool clear=true);

    ///<summary>
    ///Invokes all (dynamic, static or both) subscribers to this event.
    ///If clear is true, clears the dynamic subscribers afterwards.
    ///</summary>
    void InvokeSubscribers(bool dynamicSensitivity=true, bool staticSensitivity=true, bool clear=true);


    ///<summary>
    ///Clears all listeners from StaticSensitivity
    ///</summary>
    void ClearStaticSensitivity();

    ///<summary>
    ///Clears all listeners from DynamicSensitivity
    ///</summary>
    void ClearDynamicSensitivity();

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
