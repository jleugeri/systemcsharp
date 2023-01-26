
public class Module : IModule
{
    protected IEventLoop EventLoop;

    protected double SimulationTime { get { return EventLoop.SimulationTime; } }

    public Module(IEventLoop eventloop)
    {
        EventLoop = eventloop;
    }

    public IEvent Delay(double delay)
    {
        Event ev = new Event("Delay", EventLoop);
        ev.Notify(delay);

        return ev;
    }
}