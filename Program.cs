
using System;

/*
class MyModule
{
    public EventAwaiter ev;

    public async void Process()
    {
        ev = new EventAwaiter();
        System.Console.WriteLine("Before");
        await ev;
        System.Console.WriteLine("After");
    }
}
*/

class Module
{
    protected EventLoop eventloop;

    public double simulation_time { get {return eventloop.simulation_time;} }

    public Module(EventLoop _eventloop)
    {
        eventloop = _eventloop;
    }

}

class MyModule1 : Module
{
    public Task t1;

    public MyModule1(EventLoop _eventloop, Event ev1, Event ev2) : base(_eventloop)
    {
        t1 = Process1("t1", ev1, ev2);
    }

    async Task Process1(string name, Event ev1, Event ev2)
    {
        while (simulation_time < 100)
        {
            System.Console.WriteLine("Process 1-" + name + ": Notifying events ");
            ev1.notify(0.3);
            ev2.notify(0.7);

            await ev2;
        }
    }
}

class MyModule2 : Module
{
    public Task t2,t3; 

    public MyModule2(EventLoop _eventloop, Event ev1, Event ev2) : base(_eventloop)
    {
        t2 = Process2("t2", ev1, ev2);
        t3 = Process2("t3", ev2, ev1);
    }

    async Task Process2(string name, Event ev1, Event ev2)
    {
        System.Console.WriteLine("Process 2-" + name + ": init ");
        await ev1;
        while (true)
        {
            System.Console.WriteLine("Process 2-" + name + ": Step a ");
            await ev1;
            System.Console.WriteLine("Process 2-" + name + ": Step b ");
            await ev2;
            System.Console.WriteLine("Process 2-" + name + ": Step c ");
            await ev1;
        }
    }
}

class Program
{
    public static void Main(string[] args)
    {
        EventLoop el = new EventLoop();
        Event ev1 = new Event("ev1", el);
        Event ev2 = new Event("ev2", el);

        MyModule1 mod1 = new MyModule1(el, ev1, ev2);
        MyModule2 mod2 = new MyModule2(el, ev1, ev2);

        el.run();

    }


}