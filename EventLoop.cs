class EventLoop
{
    public struct SimTime : IComparable<SimTime>, IEquatable<object>
    {
        public double time;
        public uint delta_cycle;
        public SimTime(double _time, uint _delta_cycle)
        {
            time = _time;
            delta_cycle = _delta_cycle;
        }


        // Comparison operators

        public int CompareTo(SimTime y)
        {
            int cmp = time.CompareTo(time);
            return (cmp!=0) ? cmp : delta_cycle.CompareTo(y.delta_cycle);
        }

        public bool Equals(SimTime x)    { return CompareTo(x) == 0; }
        public override bool Equals(object? obj)
        {
            return (obj != null) && (obj is SimTime) && (CompareTo((SimTime)obj) == 0);
        }
        public override int GetHashCode()
        {
            return (time,delta_cycle).GetHashCode();
        }

        public static bool operator  < (SimTime x, SimTime y) { return x.CompareTo(y)  < 0; }
        public static bool operator  > (SimTime x, SimTime y) { return x.CompareTo(y)  > 0; }
        public static bool operator <= (SimTime x, SimTime y) { return x.CompareTo(y) <= 0; }
        public static bool operator >= (SimTime x, SimTime y) { return x.CompareTo(y) >= 0; }
        public static bool operator == (SimTime x, SimTime y) { return x.CompareTo(y) == 0; }
        public static bool operator != (SimTime x, SimTime y) { return x.CompareTo(y) != 0; }
    }


    private SimTime _simulation_time;

    Action? immediate_action;

    public double simulation_time { get {return _simulation_time.time;} }
    private PriorityQueue<Tuple<Event,SimTime>,SimTime> queue;

    public EventLoop()
    {
        _simulation_time = new SimTime(0.0,0);
        immediate_action = null;
        queue = new PriorityQueue<Tuple<Event,SimTime>, SimTime>();
    }

    public int Count { get { return queue.Count; } }

    public void notify(Event ev, double dt)
    {
        SimTime next_time = new SimTime(simulation_time + dt,0);
        queue.Enqueue(new Tuple<Event,SimTime>(ev,next_time), next_time);
    }

    public void notify(Event ev)
    {
        
        // queue all dynamically scheduled actions
        if (ev.dynamic_sensitivity != null)
            immediate_action += ev.dynamic_sensitivity;

        // clear all dynamically scheduled actions
        ev.dynamic_sensitivity = null;

        // queue all statically scheduled actions
        if (ev.static_sensitivity != null)
            immediate_action += ev.static_sensitivity;
    }

    public void run()
    {
        // Run event-loop until completion
        while(Count > 0)
        {
            /*** Timed notification phase ***/
            //Find all actions in the queue to be triggered now.
            Action? current_action = immediate_action; // immediate_action is probably null here

            while(true)
            {
                //advance to next event
                (Event ev, _simulation_time) = queue.Dequeue();

                // queue all dynamically scheduled actions
                if (ev.dynamic_sensitivity != null)
                    current_action += ev.dynamic_sensitivity;

                // clear all dynamically scheduled actions
                ev.dynamic_sensitivity = null;

                // queue all statically scheduled actions
                if (ev.static_sensitivity != null)
                    current_action += ev.static_sensitivity;

                // break if we ran out of elements to check
                // or if the next element in queue happens later
                if(queue.Count == 0 || queue.Peek().Item2 > _simulation_time)
                    break;
            }

            System.Console.WriteLine("Proceeded to time " + simulation_time);

            /*** Evaluation phase ***/
            //Execute the selected actions
            //While executing, new actions might be scheduled for immediate
            //execution or a later time --> iterate until there are no more 
            do
            {
                immediate_action = null;
                if (current_action != null)
                    current_action();
            } while (immediate_action != null);

            /*** Update phase ***/
            

        }
    }

}