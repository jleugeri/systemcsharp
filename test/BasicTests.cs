namespace test;
using SystemCSharp;
using Serilog;
using Xunit.Abstractions;

public class BasicTests
{

    public BasicTests(ITestOutputHelper output)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger()
                .ForContext<BasicTests>();    
    }

    int x = 0;
    void CountFun()
    {
        x+=1;
    }
    int cnt = 0;

    [Fact]
    public void TestUpdate()
    {
        EventLoop loop = new();

        OutPort<int> po = new("Out", 0, loop);
        InPort<int> pi = new("In", loop);
        pi.Bind(po);

        Event ev1 = new("Event 1", loop);
        Event ev2 = new("Event 2", loop);

        ev1.Notify(1.0);

        ev1.DynamicSensitivity += () => po.Value = 42;


        async Task Behavior() {
            while(true)
            {
                await pi.Updated;
                Log.Verbose("Updated port!");
                cnt += 1;
                ev1.Notify(10.0);
                await ev2;
            }
        }

        var t = Behavior();

        loop.Run();

        Assert.Equal(TaskStatus.WaitingForActivation, t.Status);
        Log.Verbose("State: {state}", t.Status.ToString());

        loop.Reset();

        Assert.Equal(TaskStatus.Canceled, t.Status);
        Log.Verbose("State: {state}", t.Status.ToString());

        // Make sure the update is called exactly once
        Assert.Equal(1, cnt);

    }


    [Fact]
    public void EventTests()
    {
        EventLoop loop = new();
        
        Event ev1 = new("Event 1", loop);
        Event ev2 = new("Event 2", loop);
        Event ev3 = new("Event 3", loop);
        Event ev4 = Event.Any(new List<IEvent>{ev1, ev2, ev3}, loop);

        ev1.Notify(1.0);
        ev2.Notify(2.0);

        ev3.Notify(2.0); // multiple scheduled events -> all in queue
        ev3.Notify(3.0);

        x = 0;

        ev1.StaticSensitivity += CountFun; // test that static sensitivity works
        ev2.DynamicSensitivity += CountFun; // test that dynamic sensitivity works

        ev3.StaticSensitivity += CountFun; // multiple subscriptions have repeated effect
        ev3.StaticSensitivity += CountFun;

        EventTrace tr1 = new("Trace for ev1", ev1);
        EventTrace tr2 = new("Trace for ev2", ev2);
        EventTrace tr4 = new("Trace for ev4", ev4);

        loop.Run();

        Assert.Equal(1+1+2*2, x); // test that all three events were triggered

        Assert.Equal(new List<double>{1.0}, tr1.Times); // make sure the event was triggered at the right time
        Assert.Equal(new List<double>{2.0}, tr2.Times); // make sure the event was triggered at the right time
        Assert.Equal(new List<double>{1.0,2.0,3.0}, tr4.Times); // make sure the event was triggered at the right time

        // Continue event loop at time 3.0
        ev1.Notify(5.0);
        ev2.Notify(5.0);

        loop.Run();

        Assert.Equal(2, tr1.Times.Count);
        Assert.Equal(2, tr2.Times.Count);
        Assert.Equal(3+5.0, tr1.Times.Last()); // time should now be 3.0 + 5.0

        
        // Reset & continue event loop at time 0.0
        loop.Reset();
        ev1.Notify(5.0);
        Assert.Throws<ArgumentException>(() => loop.Run()); // This should throw an exception, because the trace has not been cleared

        // Reset & continue event loop at time 0.0
        loop.Reset();
        tr1.Clear();
        tr2.Clear();
        tr4.Clear();
        ev1.Notify(5.0);
        loop.Run(); // This should now work, because the trace has been cleared

        Assert.Single(tr1.Times);    // new event was recorded for ev1 (static sensitivity)
        Assert.Equal(5.0, tr1.Times.Last()); // time should now be 5.0
    }

    [Fact]
    public void SignalTests()
    {
        EventLoop el = new();
        Event ev1 = new("Event 1", el);
        Event ev2 = new("Event 2", el);
        Event ev3 = new("Event 3", el);
        Event ev4 = new("Event 4", el);

        Signal<int> sig1 = new("Signal 1", 10, el);
        Signal<string> sig2 = new("Signal 2", "!", el);
        
        SignalTrace<int> tr1 = new("Trace for sig1", sig1);
        EventTrace tr2 = new("Trace for sig1.Changed", sig1.Changed);
        SignalTrace<string> tr3 = new("Trace for sig2", sig2);

        ev1.StaticSensitivity += () => { sig1.Value += 1; sig2.Value += "a"; };
        ev2.StaticSensitivity += () => { sig1.Value += 2; sig2.Value += "b"; };
        ev3.StaticSensitivity += () => { sig1.Value += 3; sig2.Value += "c"; };
        ev4.StaticSensitivity += () => { sig1.Value += 4; sig2.Value += "d"; };

        ev1.Notify(1.0);
        ev2.Notify(2.0);
        ev3.Notify(3.0);
        ev4.Notify(4.0);

        el.Run();

        Assert.Equal(new List<double>{1,2,3,4}, tr2); // Note: no zero event is padded
        Assert.Equal(Enumerable.Zip(new List<double>{0,1,2,3,4}, new List<int>{10,11,13,16,20}), tr1); // note: a zero event is padded to set initial value

        Assert.Equal(Enumerable.Zip(new List<double>{0,1,2,3,4}, new List<string>{"!","!a","!ab","!abc","!abcd"}), tr3); // should also work for string type
    }


    [Fact]
    public void PortTests()
    {
        EventLoop el = new();
        OutPort<string> p1 = new("Out-Port1", "Huh?", el);
        OutPort<string> p2 = new("Out-Port2", "Huh?", el);
        InPort<string> p3 = new("In-Port1", el);
        InPort<string> p4 = new("In-Port2", el);

        p3.Bind(p1);
        p4.Bind(p2);

        SignalTrace<string> et1 = new("Trace for p3", p3.Signal);
        SignalTrace<string> et2 = new("Trace for p4", p4.Signal, true);
    
        SignalTrace<string> et3 = new("Second Trace for p4", p4.Signal, false);

        p1.Value = "SECRET!";
        p2.Value = "Huh?";

        Assert.Equal("Huh?", p3.Value); // this should return the initial value, because the event-loop hasn't run yet!
        Assert.Equal("Huh?", p4.Value); // this should return the initial value, because the event-loop hasn't run yet!

        el.Run();
        
        Assert.Equal("SECRET!", p3.Value); // this should work now, because the event-loop has run now!
        Assert.Equal("Huh?", p4.Value); // this should work now, because the event-loop has run now!

        Assert.True(p3.WasChanged);
        Assert.True(p3.WasUpdated);
        Assert.False(p4.WasChanged);
        Assert.True(p4.WasUpdated);

        Assert.Equal(new List<double>{0.0,0.0}, et1.Times); // Make sure that the update is also called
        Assert.Equal(new List<double>{0.0,0.0}, et2.Times); // Make sure that the update is also called
        Assert.Equal(new List<double>{0.0}, et3.Times); // Make sure that the update not called
        Assert.Equal(new List<string>{"Huh?","SECRET!"}, et1.Values); // Make sure that the update is also called
        Assert.Equal(new List<string>{"Huh?","Huh?"}, et2.Values); // Make sure that the update is also called
        Assert.Equal(new List<string>{"Huh?"}, et3.Values); // Make sure that the update is not called

        // make sure we can also reset the flags
        p3.WasChanged = false;
        p3.WasUpdated = false;
        p4.WasChanged = false;
        p4.WasUpdated = false;

        Assert.False(p3.WasChanged);
        Assert.False(p3.WasUpdated);
        Assert.False(p4.WasChanged);
        Assert.False(p4.WasUpdated);

    }

    class MyModule1 : Module
    {
        public Task t1;
        public Task t2;
        public OutPort<string> p1;
        
        public SignalTrace<string> logger;

        public bool WasCompleted = false;

        public MyModule1(EventLoop eventLoop, Event ev1) : base("MyModule1", eventLoop)
        {
            t1 = Process1(ev1);
            t2 = EndProc();
            p1 = new OutPort<string>("MessageOutPort", "", eventLoop);
            logger = new("Trace");

        }

        async Task Process1(Event ev1)
        {
            await EventLoop.Started;

            while (SimulationTime < 15)
            {
                ev1.Notify(0.3);
                await Delay(0.7);
                logger.Record(SimulationTime, "A");
                p1.Value = "Test Message";           
                await Delay(5.0);
                logger.Record(SimulationTime, "B");
            }
        }

        async Task EndProc()
        {
            await EventLoop.Completed;
            WasCompleted = true;
        }

        public override void Reset()
        {
            logger.Clear();
        }
    }
    
    class MyModule2 : Module
    {
        public Task t2; 
        public InPort<string> p2;

        public SignalTrace<string> logger;
        public MyModule2(EventLoop eventLoop, Event ev1) : base("MyModule2", eventLoop)
        {
            t2 = Process2("t2", ev1);
            p2 = new InPort<string>("MessageInPort", eventLoop);
            logger = new("Trace");
        }

        async Task Process2(string name, Event ev1)
        {
            await EventLoop.Started;
            System.Console.WriteLine("Process 2-" + name + ": init ");

            while (true)
            {
                await ev1;
                logger.Record(SimulationTime, "A");
                await p2.Updated;
                logger.Record(SimulationTime, "B: "+p2.Value);
                await ev1;
                logger.Record(SimulationTime, "C");
            }
        }
        public override void Reset()
        {
            logger.Clear();
        }

    }

    [Fact]
    public void ModuleTests()
    {
        EventLoop el = new EventLoop(); 
        Event ev1 = new Event("ev1", el);
        MyModule1 mod1 = new MyModule1(el, ev1);
        MyModule2 mod2 = new MyModule2(el, ev1);

        mod2.p2.Bind(mod1.p1);
        el.Run();


        SignalTrace<string> Ref1 = new("Reference");
        Ref1.Record(0.7, "A");
        Ref1.Record(5.7, "B");
        Ref1.Record(6.4, "A");
        Ref1.Record(11.4, "B");
        Ref1.Record(12.1, "A");
        Ref1.Record(17.1, "B");

        SignalTrace<string> Ref2 = new("Reference");
        Ref2.Record(0.3, "A");
        Ref2.Record(0.7, "B: Test Message");
        Ref2.Record(6.0, "C");
        Ref2.Record(11.7, "A");
        Ref2.Record(12.1, "B: Test Message");


        Assert.Equal(Ref1.Count(), mod1.logger.Count());
        foreach(var ((t1,v1),(t2,v2)) in Enumerable.Zip(Ref1, mod1.logger))
        {
            Assert.Equal(t1, t2, 5);
            Assert.Equal(v1, v2);
        }

        
        Assert.Equal(Ref2.Count(), mod2.logger.Count());
        foreach(var ((t1,v1),(t2,v2)) in Enumerable.Zip(Ref2, mod2.logger))
        {
            Assert.Equal(t1, t2, 5);
            Assert.Equal(v1, v2);
        }

        Assert.True(mod1.WasCompleted);

    }

    
    [Fact]
    public void Test1()
    {
        EventLoop el = new EventLoop();

        var inp1 = new OutPort<double>("out", 0, el);       
        var outp = new InPort<double>("in", el);    

        inp1.Bind(outp);
        var ev1 = new Event("ev1", el);
        var ev2 = new Event("ev2", el);

        SignalTrace<double> tr1 = new("Voltage trace 1", outp.Signal);
        EventTrace tr2 = new("Spike trace 1", outp.Updated);

        async Task Behavior(string proc)
        {
            
            await el.Started;
            while(true)
            {
                await ev1;
                inp1.Value = 1.0;
                ev2.Notify(1.0);
                await ev2;
                inp1.Value = 0.0;
            }
        }
        
        var res = Behavior("First try");

        for(int i=0; i<3; i++)
        {
            Log.Information("############################");
            el.Reset();

            // Task was canceled, start again!
            Assert.Equal(TaskStatus.Canceled, res.Status);
            res = Behavior($"Try {i}");
            Assert.Equal(TaskStatus.WaitingForActivation, res.Status);

            for(int j=0; j<=i; j++)
            {
                ev1.Notify(j*40 + 10.0);
                ev1.Notify(j*40 + 20.0);
                ev1.Notify(j*40 + 30.0);
            }
            tr1.Clear();
            tr2.Clear();

            
            el.Run();

            foreach(var (t,v) in Enumerable.Zip(tr1.Times, tr1.Values))
            {
                Log.Information($"@t={t}: {v}");
            }

            foreach(var t in tr2)
            {
                Log.Information($"@t={t}: Spike!");
            }


            Assert.Equal(new List<double>{0.0, 10.0,11.0,20.0,21.0,30.0,31.0,50.0,51.0,60.0,61.0,70.0,71.0,90.0,91.0,100.0,101.0,110.0,111.0}.Take(1+(i+1)*6), tr1.Times);

            Assert.Equal(new List<double>{0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0}.Take(1+(i+1)*6), tr1.Values);

            Assert.Equal(new List<double>{10.0,11.0,20.0,21.0,30.0,31.0,50.0,51.0,60.0,61.0,70.0,71.0,90.0,91.0,100.0,101.0,110.0,111.0}.Take((i+1)*6), tr2);
        }
    }

    
    [Fact]
    public void TestMaxDuration()
    {
        EventLoop el = new EventLoop();

        var inp1 = new OutPort<double>("out", 0, el);       
        var outp = new InPort<double>("in", el);    

        inp1.Bind(outp);
        var ev1 = new Event("ev1", el);
        var ev2 = new Event("ev2", el);

        SignalTrace<double> tr1 = new("Voltage trace 1", outp.Signal);
        EventTrace tr2 = new("Spike trace 1", outp.Updated);

        async Task Behavior(string proc)
        {
            
            await el.Started;
            while(true)
            {
                await ev1;
                inp1.Value = 1.0;
                ev2.Notify(1.0);
                await ev2;
                inp1.Value = 0.0;
            }
        }
        
        var res = Behavior("First try");

        for(int i=0; i<3; i++)
        {
            Log.Information("############################");
            el.Reset();

            // Task was canceled, start again!
            Assert.Equal(TaskStatus.Canceled, res.Status);
            res = Behavior($"Try {i}");
            Assert.Equal(TaskStatus.WaitingForActivation, res.Status);

            for(int j=0; j<=i; j++)
            {
                ev1.Notify(j*40 + 10.0);
                ev1.Notify(j*40 + 20.0);
                ev1.Notify(j*40 + 30.0);
            }
            tr1.Clear();
            tr2.Clear();

            
            el.Run(90.0);

            foreach(var (t,v) in Enumerable.Zip(tr1.Times, tr1.Values))
            {
                Log.Information($"@t={t}: {v}");
            }

            foreach(var t in tr2)
            {
                Log.Information($"@t={t}: Spike!");
            }


            Assert.Equal(new List<double>{0.0, 10.0,11.0,20.0,21.0,30.0,31.0,50.0,51.0,60.0,61.0,70.0,71.0,90.0}.Take(1+(i+1)*6), tr1.Times);

            Assert.Equal(new List<double>{0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0,1.0,0.0, 1.0, 0.0, 1.0}.Take(1+(i+1)*6), tr1.Values);

            Assert.Equal(new List<double>{10.0,11.0,20.0,21.0,30.0,31.0,50.0,51.0,60.0,61.0,70.0,71.0,90.0}.Take((i+1)*6), tr2);
        }
    }

    [Fact]
    public void SampleTracesTest()
    {
        SignalTrace<int> tr0 = new("Empty signal trace", null);
        SignalTrace<int> tr1 = new("Signal trace", null);
        EventTrace tr2 = new("Event trace", null);
        EventTrace tr3 = new("Empty event trace", null);
 
        foreach(var (t,v) in new List<(double,int)>{
            (1.0,0), 
            (2.0,0), 
            (3.0,0), (3.0,0), (3.0,0), 
            (4.0,0), //nothing happens to the signal until here
            (5.0,1), (5.0,0), // the signal changes back and forth here
            (6.0,1), (6.0,2), // here the signal changes twice
            (7.0,3)
        })
        {
            tr1.Record(t, v);
            tr2.Record(t);
        }

        // test the signal traces
        Assert.Equal(double.NegativeInfinity, tr0.LastChanged(0.5));

        Assert.Equal(double.NegativeInfinity, tr1.LastChanged(0.5));
        Assert.Equal(1.0, tr1.LastChanged(1.0));
        Assert.Equal(1.0, tr1.LastChanged(1.5));
        Assert.Equal(1.0, tr1.LastChanged(2.0));
        Assert.Equal(1.0, tr1.LastChanged(3.0));
        Assert.Equal(1.0, tr1.LastChanged(4.0));
        Assert.Equal(1.0, tr1.LastChanged(5.0));
        Assert.Equal(1.0, tr1.LastChanged(5.5));
        Assert.Equal(6.0, tr1.LastChanged(6.0));
        Assert.Equal(6.0, tr1.LastChanged(6.5));
        Assert.Equal(7.0, tr1.LastChanged(7.0));
        Assert.Equal(7.0, tr1.LastChanged(7.5));

        Assert.Throws<IndexOutOfRangeException>(()=>tr0.SampleAt(1.0,false));
        Assert.Throws<IndexOutOfRangeException>(()=>tr0.SampleAt(1.0,true));
        Assert.Throws<IndexOutOfRangeException>(()=>tr1.SampleAt(1.0,false));
        Assert.Equal(0, tr1.SampleAt(1.0,true));
        
        Assert.Equal(0, tr1.SampleAt(3.0,false));
        Assert.Equal(0, tr1.SampleAt(3.0,true));

        Assert.Equal(0, tr1.SampleAt(5.0,false));
        Assert.Equal(0, tr1.SampleAt(5.0,true));

        Assert.Equal(0, tr1.SampleAt(5.5,false));
        Assert.Equal(0, tr1.SampleAt(5.5,true));

        Assert.Equal(0, tr1.SampleAt(6.0,false));
        Assert.Equal(2, tr1.SampleAt(6.0,true));

        Assert.Equal(2, tr1.SampleAt(6.5,false));
        Assert.Equal(2, tr1.SampleAt(6.5,true));

        Assert.Equal(2, tr1.SampleAt(7.0,false));
        Assert.Equal(3, tr1.SampleAt(7.0,true));

        Assert.Equal(3, tr1.SampleAt(7.5,false));
        Assert.Equal(3, tr1.SampleAt(7.5,true));

        // test event trace
        Assert.True(tr2.SampleAt(1.0));
        Assert.True(tr2.SampleAt(2.0));
        Assert.True(tr2.SampleAt(3.0));
        Assert.True(tr2.SampleAt(4.0));
        Assert.True(tr2.SampleAt(5.0));
        Assert.True(tr2.SampleAt(6.0));
        Assert.True(tr2.SampleAt(7.0));

        Assert.Equal(1.0, tr2.LastChanged(1.0));
        Assert.Equal(2.0, tr2.LastChanged(2.0));
        Assert.Equal(3.0, tr2.LastChanged(3.0));
        Assert.Equal(4.0, tr2.LastChanged(4.0));
        Assert.Equal(5.0, tr2.LastChanged(5.0));
        Assert.Equal(6.0, tr2.LastChanged(6.0));
        Assert.Equal(7.0, tr2.LastChanged(7.0));

        Assert.False(tr2.SampleAt(1.5));
        Assert.False(tr2.SampleAt(2.5));
        Assert.False(tr2.SampleAt(3.5));
        Assert.False(tr2.SampleAt(4.5));
        Assert.False(tr2.SampleAt(5.5));
        Assert.False(tr2.SampleAt(6.5));
        Assert.False(tr2.SampleAt(7.5));

        Assert.Equal(double.NegativeInfinity, tr2.LastChanged(0.5));
        Assert.Equal(1.0, tr2.LastChanged(1.5));
        Assert.Equal(2.0, tr2.LastChanged(2.5));
        Assert.Equal(3.0, tr2.LastChanged(3.5));
        Assert.Equal(4.0, tr2.LastChanged(4.5));
        Assert.Equal(5.0, tr2.LastChanged(5.5));
        Assert.Equal(6.0, tr2.LastChanged(6.5));
        Assert.Equal(7.0, tr2.LastChanged(7.5));

        Assert.False(tr3.SampleAt(7.5));
        Assert.Equal(double.NegativeInfinity, tr3.LastChanged(0.5));

        Assert.True(tr2.SampleAt(1.5, -0.6));
        Assert.True(tr2.SampleAt(1.5, 0.0, 0.6));
        Assert.False(tr2.SampleAt(1.0, 0.1,-0.1)); // impossible condition


    }
}