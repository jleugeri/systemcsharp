namespace SystemCSharp;
using Serilog;

public class EventLoop : IEventLoop
{
    protected ISet<IEvent> RegisteredEvents;

    protected ISet<IEvent> ImmediateEvents;
    protected ISet<IUpdate> Updates;
    protected PriorityQueue<Tuple<IEvent, double>, double> Queue;

    public double SimulationTime { get; protected set; }
    public int Count { get { return Queue.Count; } }

    public IEvent Started {get; protected set;}

    public IEvent Completed {get; protected set;}

    protected CancellationTokenSource CancellationTokenSource = new();
    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public EventLoop()
    {
        SimulationTime = 0.0;
        RegisteredEvents = new HashSet<IEvent>();
        ImmediateEvents = new HashSet<IEvent>();
        Updates = new HashSet<IUpdate>();
        Queue = new PriorityQueue<Tuple<IEvent, double>, double>();
        Started = new Event("StartedEvent", this);
        Completed = new Event("CompletedEvent", this);
    }


    public void Notify(IEvent ev, double dt)
    {
        double nextTime = SimulationTime + dt;
        Queue.Enqueue(new Tuple<IEvent, double>(ev, nextTime), nextTime);
    }

    public void Notify(IEvent ev)
    {
        ImmediateEvents.Add(ev);
    }

    public void RequestUpdate(IUpdate obj)
    {
        Updates.Add(obj);
    }

    public void Reset()
    {
        // trigger cancellation token, which should cause all subsequent
        // calls to GetResult() to fail
        CancellationTokenSource.Cancel();

        // Now "flush" all dynamically pending tasks and let them get cancelled
        foreach(var ev in RegisteredEvents)
        {
            ev.DynamicSensitivity?.Invoke();
        }

        // Recreate the CancellationTokenSource
        CancellationTokenSource.TryReset();
        //CancellationTokenSource.Dispose();
        //CancellationTokenSource = new();

        SimulationTime = 0.0;
        ImmediateEvents.Clear();
        Updates.Clear();
        Queue.Clear();
    }

    public void UpdatePhase()
    {
        // Execute all updates
        foreach(var update in Updates)
        {
            update.ApplyUpdate();
        }

        // Clear list of pending updates
        Updates.Clear();
    }


    public void Run()
    {
        Action? currentAction = null;

        void ScheduleForExecution(IEvent ev)
        {

            // queue all dynamically scheduled actions
            if (ev.DynamicSensitivity != null)
                currentAction += ev.DynamicSensitivity;

            // clear all dynamically scheduled actions
            ev.DynamicSensitivity = null;

            // queue all statically scheduled actions
            if (ev.StaticSensitivity != null)
                currentAction += ev.StaticSensitivity;
        }

        // Run initial update phase
        UpdatePhase();

        // Trigger started event
        Started.Notify(0.0);

        // if we only processed the completed event during the event loop, 
        // we should not trigger another completed event to avoid an endless loop
        bool onlyProcessedCompletedEvent = false;

        // Run event-loop until completion
        while (Count > 0)
        {

            /*** Timed notification phase ***/
            //Find all actions in the queue to be triggered now.
            currentAction = null;
            
            // First, take care of immediate events
            foreach(var ev in ImmediateEvents)
            {
                Log.Logger.Verbose("Event Loop: Executing immediate event '{name}' at time {time}.", ev.Name, SimulationTime);

                // note if this was something else than the completed event
                if(ev!=Completed)
                    onlyProcessedCompletedEvent = false;

                ScheduleForExecution(ev);
            }
            // Clear immediate events now
            ImmediateEvents.Clear();

            // second, take care of queued events
            while (true)
            {
                //advance to next event
                (IEvent ev, SimulationTime) = Queue.Dequeue();
                Log.Logger.Verbose("Event Loop: Executing event '{name}' at time {time}.", ev.Name, SimulationTime);

                // note if this was something else than the completed event
                if(ev!=Completed)
                    onlyProcessedCompletedEvent = false;

                ScheduleForExecution(ev);

                // break if we ran out of elements to check
                // or if the next element in queue happens later
                if (Queue.Count == 0 || Queue.Peek().Item2 > SimulationTime)
                    break;
            }

            Log.Logger.Verbose("Event Loop: Starting evaluation phase for time {time}", SimulationTime);

            /*** Evaluation phase ***/
            //Execute the selected actions
            //While executing, new actions might be scheduled for immediate
            //execution or a later time --> iterate until there are no more 
            int deltaCycle = 0;
            do
            {
                Log.Logger.Verbose("Event Loop: Starting delta cycle {deltaCycle} for time {time}", deltaCycle, SimulationTime);
                
                // Take care of any immediate events from the previous delta cycle, if any
                foreach(var ev in ImmediateEvents)
                {
                    Log.Logger.Verbose("Event Loop: Executing immediate event '{name}' in delta cycle {deltaCycle} at time {time}.", ev.Name, deltaCycle, SimulationTime);

                    // note if this was something else than the completed event
                    if(ev!=Completed)
                        onlyProcessedCompletedEvent = false;

                    ScheduleForExecution(ev);
                }
                // Clear immediate events now
                ImmediateEvents.Clear();
                
                // Execute the currently pending actions
                if (currentAction != null)
                    currentAction();

                // Clear the currently pending actions
                currentAction = null;

                // Repeat until there are no more immediately pending actions -> we can move to the next time-stamp
            } while (ImmediateEvents.Count != 0);

            /*** Update phase ***/
            UpdatePhase();
            
            // If we did anything more than process the completed event this loop, 
            // and we are now at the end of the queue, then trigger the completed event.
            if(!onlyProcessedCompletedEvent && Count == 0)
            {
                onlyProcessedCompletedEvent = true;
                Completed.Notify(0.0);
            }
        }
    }

    public void RegisterEvent(IEvent ev)
    {
        RegisteredEvents.Add(ev);
    }

    public void UnregisterEvent(IEvent ev)
    {
        RegisteredEvents.Remove(ev);
    }
}