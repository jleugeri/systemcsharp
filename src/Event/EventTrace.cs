using System.Collections;
namespace SystemCSharp;

public class EventTrace : IEventTrace
{
    public string Name { get; protected set; }
    public IEvent? Event { get; protected set; } = null;
    public List<double> Times { get; protected set; }

    public EventTrace(string name, IEvent? Event = null)
    {
        Name = name;
        Times = new();

        Trace(Event);
    }

    public void Trace(IEvent? _event = null)
    {
        var oldEvent = Event;
        Event = _event;

        if (oldEvent != null && oldEvent != Event)
        {
            oldEvent.StaticSensitivity -= AddFromEvent;
        }

        if (Event != null && Event != oldEvent)
        {
            Event.StaticSensitivity += AddFromEvent;
        }
    }

    private void AddFromEvent()
    {
        Record(Event!.EventLoop.SimulationTime);
    }

    public void Record(double time)
    {
        if(Times.Count>0 && Times.Last() > time)
            throw new ArgumentException("Times in trace must always increase!");
            
        Times.Add(time);
    }

    public void Clear()
    {
        Times.Clear();
    }

    public IEnumerator<double> GetEnumerator()
    {
        return Times.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

}