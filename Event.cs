using System.Runtime.CompilerServices;


///<summary>
/// An awaitable event for simulation of asynchronous systems.
///</summary>
class Event
{
    ///<summary>
    ///Holds all the actions scheduled for execution upon the next trigger of 
    ///this event.
    ///</summary>
    private List<Action> scheduled_actions;

    ///<summary>Holds a reference to the `EventLoop`.</summary>
    private EventLoop eventloop;


    ///<summary>Name of the event.</summary>
    private string name;

    ///<summary>Constructs an event with the given name.</summary>
    public Event(string _name, EventLoop _eventloop)
    {
        name = _name;
        eventloop = _eventloop;

        // Begin with an empty list of actions
        scheduled_actions = new List<Action>();
    }

    ///<summary>
    ///Registers an `Action` for execution upon the next trigger of this event
    ///</summary>
    public void schedule(Action action)
    {
        scheduled_actions.Add(action);
    }

    ///<summary>Notify the event-loop to schedule this event with `delay`</summary>
    public void notify(double delay)
    {
        eventloop.notify(this, delay);
    }

    ///<summary>
    ///Triggers the event, executes scheduled actions and clears the schedule.
    ///While executing the scheduled actions, new actions might be scheduled
    ///for the next time the event is `trigger`ed.
    ///</summary>
    public void trigger()
    {
        List<Action> current_actions = scheduled_actions;
        scheduled_actions = new List<Action>();

        foreach (Action action in current_actions)
        {
            action();
        }
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

    ///<summary>///Awaiter for `Event` class.///</summary>
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
                parent.schedule(continuation);
        }

        ///<summary>Returns void</summary>
        public void GetResult()
        {}
    }
}