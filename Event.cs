using System.Runtime.CompilerServices;


///<summary>
/// An awaitable event for simulation of asynchronous systems.
///</summary>
class Event
{

    ///<summary>
    ///Holds all the statically scheduled actions for execution upon 
    ///the next trigger of this event.
    ///</summary>
    public Action? static_sensitivity;
    ///<summary>
    ///Holds all the dynamically scheduled actions for execution upon 
    ///the next trigger of this event.
    ///</summary>
    public Action? dynamic_sensitivity;

    ///<summary>Holds a reference to the `EventLoop`.</summary>
    protected EventLoop eventloop;

    ///<summary>Name of the event.</summary>
    protected string name;

    ///<summary>Constructs an event with the given name.</summary>
    public Event(string _name, EventLoop _eventloop)
    {
        name = _name;
        eventloop = _eventloop;
        static_sensitivity = null;
        dynamic_sensitivity = null;
    }

    ///<summary>
    ///Notify the event-loop to schedule this event with `delay`
    ///</summary>
    public void notify(double delay)
    {
        eventloop.notify(this, delay);
    }

    ///<summary>
    ///Triggers the event immediately.
    ///</summary>
    public void notify()
    {
        eventloop.notify(this);
    }

    ///<summary>
    ///Returns a new `EventAwaiter` object that can be `await`ed.
    ///When the returned object is `await`ed, it registers the given 
    ///continuation action with the parent `Event` object.
    ///</summary>
    public EventAwaiter GetAwaiter()
    {
        return new EventAwaiter(this);
    }

    ///<summary>
    ///Custom `Awaiter` for the `Event` class.
    ///This class makes it possible to `await` an `Event`, which adds the
    ///continuation after `await` to the dynamic sensitivity of the `Event`.
    ///</summary>
    public struct EventAwaiter : INotifyCompletion
    {
        ///<summary>Reference to the awaiter's parent event.</summary>
        private Event parent;

        ///<summary>Constructs an `EventAwaiter` for an `Event _parent`</summary>
        public EventAwaiter(Event _parent)
        {
            parent = _parent;
        }

        ///<summary>
        ///Returns false, and thus alway yields control when `await`ed.
        ///</summary>
        public bool IsCompleted => false;

        ///<summary>
        ///On `await`ing an `EventAwaiter` object in an async function, the
        ///continuation of the function is registered with the parent `Event` 
        ///object, and executed once it is triggered.
        ///</summary>
        public void OnCompleted(Action continuation)
        {
            if (null != continuation)
                parent.dynamic_sensitivity += continuation;
        }

        ///<summary>Returns void</summary>
        public void GetResult()
        { }
    }
}