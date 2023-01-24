class Module
{
    protected EventLoop EventLoop;

    public double SimulationTime { get {return EventLoop.SimulationTime;} }

    public Module(EventLoop eventloop)
    {
        EventLoop = eventloop;
    }

    public Event Delay(double delay)
    {
        Event ev = new Event("Delay", EventLoop);
        ev.Notify(delay);
        
        return ev;
    }
}