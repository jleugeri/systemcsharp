# SystemC#


# Example
To show how this module can be used, let's look at a simple example.

## MyModule1: Module with Process 1
The first module defines a single process (t1) and a single output port (p1).
The process periodically triggers an external event (ev1) with 0.3 seconds delay, waits for 0.7 seconds, then sends out a message on the output port (`p1,data = ...`), and then waits for another 5 seconds.
```C#
class MyModule1 : Module
{
    public Task t1;
    public OutPort<string> p1;

    public MyModule1(EventLoop eventLoop, Event ev1) : base(eventLoop)
    {
        t1 = Process1("t1", ev1);
        p1 = new OutPort<string>("MessageOutPort", "");
    }

    async Task Process1(string name, Event ev1)
    {
        while (SimulationTime < 100)
        {
            System.Console.WriteLine("Process 1-" + name + ": Notifying events ");
            ev1.Notify(0.3);
            await Delay(0.7);
            p1.data = $"Test Message: {SimulationTime}";           
            await Delay(5.0);
        }
    }
}
```

## MyModule2: Module with Process 2
The second module defines a second process (t2) and a single input port (p2).
The process periodically waits for the external event (ev1) triggered by the first process and waits for a message on the input port, sent by the first process.
In addition to the dynamic sensitivity (i.e. the explicit `await p2.Updated`), the lambda function `()=>System.Console.WriteLine("PORT 2 was updated!")` is added to the static sensitivity list of `p2.Updated`, which is triggered whenever `p2` is written to.
```C#
class MyModule2 : Module
{
    public Task t2; 
    public InPort<string> p2;

    public MyModule2(EventLoop eventLoop, Event ev1) : base(eventLoop)
    {
        t2 = Process2("t2", ev1);
        p2 = new InPort<string>("MessageInPort", eventLoop);
        p2.Updated.StaticSensitivity += ()=>System.Console.WriteLine("PORT 2 was updated!");
    }

    async Task Process2(string name, Event ev1)
    {
        System.Console.WriteLine("Process 2-" + name + ": init ");
        await ev1;
        while (true)
        {
            System.Console.WriteLine("Process 2-" + name + ": Step a ");
            await ev1;
            System.Console.WriteLine("Process 2-" + name + ": Step b ");
            await p2.Updated;
            System.Console.WriteLine("Process 2-" + name + ": Step c ");
            await ev1;
        }
    }
}
```

## Program: Container for an instance of MyModule1 and MyModule2
The main program instantiates the eventloop (el), the event (ev1), the two modules defined above 
and then binds the input port (p2) of module 2 (mod2) to the output port (p1) of module 1 (mod1).
Finally, it starts execution by calling `Run()` on the event loop.
```C#
class Program
{
    public static void Main(string[] args)
    {
        EventLoop el = new EventLoop(); 
        Event ev1 = new Event("ev1", el);
        MyModule1 mod1 = new MyModule1(el, ev1);
        MyModule2 mod2 = new MyModule2(el, ev1);

        mod2.p2.Bind(mod1.p1);
        el.Run();
    }
}
```