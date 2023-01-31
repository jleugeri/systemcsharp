namespace test;
using SystemCSharp;

public class BasicTests
{
    int x = 0;
    void CountFun()
    {
        x+=1;
    }

    [Fact]
    public void EventTests()
    {
        EventLoop loop = new();
        
        Event ev1 = new("Event 1", loop);
        Event ev2 = new("Event 2", loop);
        Event ev3 = new("Event 3", loop);

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

        loop.Run();

        Assert.Equal(1+1+2*2, x); // test that all three events were triggered

        Assert.Single(tr1.Times); // make sure that the event was only triggered once
        Assert.Single(tr2.Times); // make sure that the event was only triggered once

        Assert.Equal(1.0, tr1.Times.Last()); // make sure the event was triggered at the right time
        Assert.Equal(2.0, tr2.Times.Last()); // make sure the event was triggered at the right time

        // Continue event loop at time 3.0
        ev1.Notify(5.0);
        ev2.Notify(5.0);
        loop.Run();

        Assert.Equal(2, tr1.Times.Count);    // new event was recorded for ev1 (static sensitivity)
        Assert.Equal(2, tr2.Times.Count);    // no new event was recorded for ev1 (dynamic sensitivity)
        Assert.Equal(3+5.0, tr1.Times.Last()); // time should now be 3.0 + 5.0

        
        // Reset & continue event loop at time 0.0
        loop.Reset();
        ev1.Notify(5.0);
        Assert.Throws<ArgumentException>(() => loop.Run()); // This should throw an exception, because the trace has not been cleared

        // Reset & continue event loop at time 0.0
        loop.Reset();
        tr1.Clear();
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
        OutPort<string> p1 = new("Out-Port", "Huh?", el);
        InPort<string> p2 = new("In-Port", el);

        p2.Bind(p1);

        EventTrace et = new("Trace for p2", p2.Updated);

        p1.Data = "SECRET!";

        Assert.Null(p2.Data); // this shouldn't work, because the event-loop hasn't run yet!

        el.Run();
        Assert.Equal("SECRET!", p2.Data); // this should work, because the event-loop has run now!
        Assert.Equal(new List<double>{0.0}, et.Times); // Make sure that the update is also called
    }

    class MyModule1 : Module
    {
        public Task t1;
        public OutPort<string> p1;
        
        public SignalTrace<string> logger;

        public MyModule1(EventLoop eventLoop, Event ev1) : base("MyModule1", eventLoop)
        {
            t1 = Process1("t1", ev1);
            p1 = new OutPort<string>("MessageOutPort", "", eventLoop);
            logger = new("Trace");
        }

        async Task Process1(string name, Event ev1)
        {
            while (SimulationTime < 15)
            {
                ev1.Notify(0.3);
                await Delay(0.7);
                logger.Add(SimulationTime, "A");
                p1.Data = "Test Message";           
                await Delay(5.0);
                logger.Add(SimulationTime, "B");
            }
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
            System.Console.WriteLine("Process 2-" + name + ": init ");
            while (true)
            {
                await ev1;
                logger.Add(SimulationTime, "A");
                await p2.Updated;
                logger.Add(SimulationTime, "B: "+p2.Data);
                await ev1;
                logger.Add(SimulationTime, "C");
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
            Ref1.Add(0.7, "A");
            Ref1.Add(5.7, "B");
            Ref1.Add(6.4, "A");
            Ref1.Add(11.4, "B");
            Ref1.Add(12.1, "A");
            Ref1.Add(17.1, "B");

            SignalTrace<string> Ref2 = new("Reference");
            Ref2.Add(0.3, "A");
            Ref2.Add(0.7, "B: Test Message");
            Ref2.Add(6.0, "C");
            Ref2.Add(11.7, "A");
            Ref2.Add(12.1, "B: Test Message");

            foreach(var ((t1,v1),(t2,v2)) in Enumerable.Zip(Ref1, mod1.logger))
            {
                Assert.Equal(t1, t2, 5);
                Assert.Equal(v1, v2);
            }

            
            foreach(var ((t1,v1),(t2,v2)) in Enumerable.Zip(Ref2, mod2.logger))
            {
                Assert.Equal(t1, t2, 5);
                Assert.Equal(v1, v2);
            }

    }
}