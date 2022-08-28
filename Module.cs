class Module
{
    protected EventLoop eventloop;

    public double simulation_time { get {return eventloop.simulation_time;} }

    public Module(EventLoop _eventloop)
    {
        eventloop = _eventloop;
    }

    public Event delay(double _delay)
    {
        Event ev = new Event("Delay", eventloop);
        ev.notify(_delay);
        
        return ev;
    }
}