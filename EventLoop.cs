class EventLoop
{
    Action? ImmediateAction;

    public double SimulationTime { get; private set; }
    private PriorityQueue<Tuple<Event,double>,double> Queue;

    public EventLoop()
    {
        SimulationTime = 0.0;
        ImmediateAction = null;
        Queue = new PriorityQueue<Tuple<Event,double>, double>();
    }

    public int Count { get { return Queue.Count; } }

    public void Notify(Event ev, double dt)
    {
        double nextTime = SimulationTime + dt;
        Queue.Enqueue(new Tuple<Event,double>(ev,nextTime), nextTime);
    }

    public void Notify(Event ev)
    {
        
        // queue all dynamically scheduled actions
        if (ev.DynamicSensitivity != null)
            ImmediateAction += ev.DynamicSensitivity;

        // clear all dynamically scheduled actions
        ev.DynamicSensitivity = null;

        // queue all statically scheduled actions
        if (ev.StaticSensitivity != null)
            ImmediateAction += ev.StaticSensitivity;
    }

    public void Run()
    {
        // Run event-loop until completion
        while(Count > 0)
        {
            /*** Timed notification phase ***/
            //Find all actions in the queue to be triggered now.
            Action? currentAction = ImmediateAction; // ImmediateAction is probably null here

            while(true)
            {
                //advance to next event
                (Event ev, SimulationTime) = Queue.Dequeue();

                // queue all dynamically scheduled actions
                if (ev.DynamicSensitivity != null)
                    currentAction += ev.DynamicSensitivity;

                // clear all dynamically scheduled actions
                ev.DynamicSensitivity = null;

                // queue all statically scheduled actions
                if (ev.StaticSensitivity != null)
                    currentAction += ev.StaticSensitivity;

                // break if we ran out of elements to check
                // or if the next element in queue happens later
                if(Queue.Count == 0 || Queue.Peek().Item2 > SimulationTime)
                    break;
            }

            System.Console.WriteLine("Proceeded to time " + SimulationTime);

            /*** Evaluation phase ***/
            //Execute the selected actions
            //While executing, new actions might be scheduled for immediate
            //execution or a later time --> iterate until there are no more 
            do
            {
                ImmediateAction = null;
                if (currentAction != null)
                    currentAction();
            } while (ImmediateAction != null);

            /*** Update phase ***/
            

        }
    }

}