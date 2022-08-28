class EventLoop
{
    public double simulation_time { get; private set; }
    private PriorityQueue<Tuple<Event,double>,double> queue;

    public EventLoop()
    {
        simulation_time = 0.0;
        queue = new PriorityQueue<Tuple<Event,double>, double>();
    }

    public int Count { get { return queue.Count; } }

    public void notify(Event ev, double dt)
    {
        double next_time = simulation_time + dt;
        queue.Enqueue(new Tuple<Event,double>(ev,next_time), next_time);
    }

    public Event next_event()
    {
        (Event ev, double time) = queue.Dequeue();
        simulation_time = time;
        return ev;
    }

    public void run()
    {
        while(Count >  0)
        {
            Event ev = next_event();
            System.Console.WriteLine("Proceeded to time " + simulation_time);
            
            ev.trigger();
        }
    }

}