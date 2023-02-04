namespace SystemCSharp;
///<summary>
///Represents a shared value that can be accessed by multiple sources/sinks, 
///and for which the accesses need to be timed correctly.
///For example, this includes signals that require an (infinitesimal) amount of simulated time to change, 
///e.g. a signal that is accessed by two different processes with the same module,
///or a signal that is transmitted between an output and an input port of two different modules.
///</summary>
public interface ISignal<DataT> : IUpdate where DataT : IEquatable<DataT>
{
    ///<summary>
    ///The name of the signal
    ///</summary>
    string Name { get; }
    
    ///<summary>
    ///Event that is triggered whenver the values have been updated, regardless of whether the value actually changed.
    ///</summary>
    Event Updated { get; }
    
    ///<summary>
    ///Event that is triggered whenver the values have been changed by an update.
    ///</summary>
    Event Changed { get; }
    
    ///<summary>
    ///Flag thas is set whenver the values have been updated, regardless of whether the value actually changed.
    ///</summary>
    bool WasUpdated { get; set; }
    
    ///<summary>
    ///Flag that is set whenver the values have been changed by an update.
    ///</summary>
    bool WasChanged { get; set; }
    
    ///<summary>
    ///The current value of the signal
    ///</summary>
    DataT Value { get; set; }

    ///<summary>
    ///Resets the signal back to its initial value
    ///</summary>
    void Reset();
}
