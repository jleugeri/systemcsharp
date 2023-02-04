namespace SystemCSharp;

///<summary>
///Records the times and values of any update or change of a given signal (if any).
///</summary>
public interface ISignalTrace<T> : IEnumerable<(double Time, T Value)> where T : IEquatable<T>
{
    ///<summary>
    ///Name of the trace
    ///</summary>
    string Name { get; }

    ///<summary>
    ///The underlying signal being traced (if any)
    ///</summary>
    ISignal<T>? Signal { get; }

    ///<summary>
    ///The recorded times where the signal was updated or changed
    ///</summary>
    List<double> Times { get; }

    ///<summary>
    ///The recorded values to which the signal was updated or changed
    ///</summary>
    List<T> Values { get; }

    ///<summary>
    ///A parameter that determines whether all updates, including those that leave the value unchanged, 
    ///should be recorded, or only those that change the value.
    ///</summary>
    bool TraceAllUpdates { get; }

    ///<summary>
    ///Records the given `value` at the given `time`.
    ///</summary>
    void Record(double time, T value);

    ///<summary>
    ///Clears the trace entirely.
    ///</summary>
    void Clear();

    ///<summary>
    ///Starts tracing the given `signal`, if any. Stops tracing the previously traced signal, if any.
    ///</summary>
    void Trace(ISignal<T>? signal = null);
}
