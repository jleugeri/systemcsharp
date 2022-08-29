class EventLoop
{
    public struct SimTime
    {
        public class Comparer : Comparer<SimTime>
        {
            // Compares by Length, Height, and Width.
            public override int Compare(SimTime x, SimTime y)
            {
                int cmp = x.time.CompareTo(y.time);
                return (cmp!=0) ? cmp : x.delta_cycle.CompareTo(y.delta_cycle);
            }
        }

        public static Comparer GetComparer()
        {
            return new Comparer();
        }

        public double time;
        public uint delta_cycle;
        public SimTime(double _time, uint _delta_cycle)
        {
            time = _time;
            delta_cycle = _delta_cycle;
        }
    }


    private SimTime _simulation_time;

    public double simulation_time { get {return _simulation_time.time;} }
    private PriorityQueue<Tuple<Event,SimTime>,SimTime> queue;

    public EventLoop()
    {
        _simulation_time = new SimTime(0.0,0);
        queue = new PriorityQueue<Tuple<Event,SimTime>, SimTime>(SimTime.GetComparer());
    }

    public int Count { get { return queue.Count; } }

    public void notify(Event ev, double dt)
    {
        SimTime next_time = new SimTime(simulation_time + dt,0);
        queue.Enqueue(new Tuple<Event,SimTime>(ev,next_time), next_time);
    }

    public Event next_event()
    {
        Tuple<Event,SimTime> tmp = queue.Dequeue();
        _simulation_time = tmp.Item2;
        return tmp.Item1;
    }

    public void run()
    {
        while(Count > 0)
        {
            Event ev = next_event();
            System.Console.WriteLine("Proceeded to time " + simulation_time);
            
            ev.notify();
        }
    }

}