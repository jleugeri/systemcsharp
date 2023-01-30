public interface IModule
{
    IEventLoop EventLoop { get; }

    IEvent Delay(double delay);

    void Reset();
}
