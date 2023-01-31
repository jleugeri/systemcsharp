using System.Runtime.CompilerServices;

namespace SystemCSharp;

///<summary>
/// A basic implementation of IEvent that can be awaited for simulation of asynchronous systems.
///</summary>
public class Event : IEvent
{
    public static Event Any(List<IEvent> events, IEventLoop eventLoop)
    {
        Event e = new("Any-Event", eventLoop);

        foreach(var other in events)
        {
            other.StaticSensitivity += e.Notify;
        }

        return e;
    }


    public Action? StaticSensitivity { get; set; }


    public Action? DynamicSensitivity { get; set; }

    public string Name { get; }

    public IEventLoop EventLoop { get; protected set; }

    public Event(string name, IEventLoop eventLoop)
    {
        Name = name;
        EventLoop = eventLoop;
        StaticSensitivity = null;
        DynamicSensitivity = null;
    }

    public void Notify(double delay)
    {
        EventLoop.Notify(this, delay);
    }

    ///<summary>
    ///Triggers the event immediately.
    ///</summary>
    public void Notify()
    {
        EventLoop.Notify(this);
    }

    ///<summary>
    ///Returns a new `EventAwaiter` object that can be `await`ed.
    ///When the returned object is `await`ed, it registers the given 
    ///continuation action with the parent `Event` object.
    ///</summary>
    public IEventAwaiter GetAwaiter()
    {
        return new EventAwaiter(this);
    }

    ///<summary>
    ///Custom `Awaiter` for the `Event` class.
    ///This class makes it possible to `await` an `Event`, which adds the
    ///continuation after `await` to the dynamic sensitivity of the `Event`.
    ///</summary>
    public struct EventAwaiter : IEventAwaiter
    {
        ///<summary>Reference to the awaiter's parent event.</summary>
        private Event Parent;

        ///<summary>Constructs an `EventAwaiter` for an `Event parent`</summary>
        public EventAwaiter(Event parent)
        {
            Parent = parent;
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
                Parent.DynamicSensitivity += continuation;
        }

        ///<summary>Returns void</summary>
        public void GetResult()
        { }
    }
}