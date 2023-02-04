namespace SystemCSharp;

///<summary>
///Modules encapsulate independent parts of a system.
///The behavior of modules can be defined by `async Task`s, which can `await` events, e.g. `Delay(dt)`.
///Modules should only communicate with each other via connected ports.
///Note: Implementations should inherit from the abstract class `Module`.
///</summary>
public interface IModule
{
    ///<summary>
    ///The event loop that this module is managed by
    ///</summary>
    IEventLoop EventLoop { get; }

    ///<summary>
    ///Returns an event that triggers after the given `delay` (0.0 = in the next delta cycle).
    ///Since this creates a new event object every time, multiple processes of the same module
    ///can independently await a different Delay.
    ///</summary>
    IEvent Delay(double delay);

    ///<summary>
    ///Resets the module back into its initial state, ready to re-run a simulation.
    ///Note: since this is highly specific, each concrete sub-class of module must overwrite this!
    ///</summary>
    void Reset();
}
