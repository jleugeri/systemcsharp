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
        if(Times.Count>0 && Times.Last() > time+double.Epsilon)
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

    public bool SampleAt(double time, double tolerance = -1E-08, double toleranceAcausal = 1E-08) => 
        LastChanged(time + toleranceAcausal) >= time + tolerance;
        
    public double LastChanged(double time)
    {
        // get index of the event or get the next event after
        var indexOrNext = Times.BinarySearch(time);

        // Hit the exact time!
        if(indexOrNext >= 0) return time;

        // the element was not found -> binary complement holds the next larger hit or Times.Count
        indexOrNext = ~indexOrNext;

        // if the next larger index is 0, there was nothing before time
        if(indexOrNext == 0) return double.NegativeInfinity;

        return Times[indexOrNext-1];
    }
}