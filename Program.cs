class MyModule1 : Module
{
    public Task t1;
    public OutPort<string> p1;

    public MyModule1(EventLoop _eventloop, Event ev1) : base(_eventloop)
    {
        t1 = Process1("t1", ev1);
        p1 = new OutPort<string>("MessageOutPort", "");
    }

    async Task Process1(string name, Event ev1)
    {
        while (simulation_time < 100)
        {
            System.Console.WriteLine("Process 1-" + name + ": Notifying events ");
            ev1.notify(0.3);

            await delay(0.7);
            p1.data = $"Test Message: {simulation_time}";
            
            await delay(5.0);
        }
    }
}

class MyModule2 : Module
{
    public Task t2; 
    public InPort<string> p2;

    public MyModule2(EventLoop _eventloop, Event ev1) : base(_eventloop)
    {
        t2 = Process2("t2", ev1);
        p2 = new InPort<string>("MessageInPort", _eventloop);
        p2.Updated.static_sensitivity += ()=>System.Console.WriteLine("PORT 2 was updated!");
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


class Program
{
    public static void Main(string[] args)
    {
        EventLoop el = new EventLoop(); 

        Event ev1 = new Event("ev1", el);

        MyModule1 mod1 = new MyModule1(el, ev1);

        MyModule2 mod2 = new MyModule2(el, ev1);


        mod2.p2.bind(mod1.p1);


        el.run();
    }


}