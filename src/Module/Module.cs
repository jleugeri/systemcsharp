
public abstract class Module : IModule
{
    public IEventLoop EventLoop { get; protected set; }

    protected double SimulationTime { get { return EventLoop.SimulationTime; } }

    public string Name { get; }

    public Module(string name, IEventLoop eventloop)
    {
        Name = name;
        EventLoop = eventloop;
    }

    public IEvent Delay(double delay)
    {
        Event ev = new Event("Delay", EventLoop);
        ev.Notify(delay);

        return ev;
    }

    public abstract void Reset();
}