namespace SystemCSharp;
///<summary>
///Basic implementation of IEventLoop that can schedule IEvents.
///</summary>
public class EventLoop : IEventLoop
{
    Action? ImmediateAction;
    Action? UpdateAction;

    public double SimulationTime { get; private set; }
    public int Count { get { return Queue.Count; } }

    private PriorityQueue<Tuple<IEvent, double>, double> Queue;

    public EventLoop()
    {
        SimulationTime = 0.0;
        ImmediateAction = null;
        Queue = new PriorityQueue<Tuple<IEvent, double>, double>();
    }


    public void Notify(IEvent ev, double dt)
    {
        double nextTime = SimulationTime + dt;
        Queue.Enqueue(new Tuple<IEvent, double>(ev, nextTime), nextTime);
    }

    public void Notify(IEvent ev)
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

    public void RequestUpdate(IUpdate obj)
    {
        UpdateAction += obj.ApplyUpdate;
    }

    public void Reset()
    {
        SimulationTime = 0.0;
        ImmediateAction = null;
        Queue.Clear();
    }

    public void UpdatePhase()
    {
        // Execute all updates
        if(UpdateAction != null)
            UpdateAction();

        // Clear list of pending updates
        UpdateAction = null;
    }

    public void Run()
    {
        // Run initial update phase
        UpdatePhase();

        // Run event-loop until completion
        while (Count > 0)
        {
            /*** Timed notification phase ***/
            //Find all actions in the queue to be triggered now.
            Action? currentAction = ImmediateAction; // ImmediateAction is probably null here

            while (true)
            {
                //advance to next event
                (IEvent ev, SimulationTime) = Queue.Dequeue();

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
                if (Queue.Count == 0 || Queue.Peek().Item2 > SimulationTime)
                    break;
            }

            //System.Console.WriteLine("Proceeded to time " + SimulationTime);

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
            UpdatePhase();

        }
    }

}