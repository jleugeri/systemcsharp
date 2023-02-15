namespace SystemCSharp;

///<summary>
///Traces the times of occurence of an event.
///</summary>
public interface IEventTrace : IEnumerable<double>
{
    ///<summary>
    ///The name of the trace
    ///</summary>
    string Name { get; }

    ///<summary>
    ///The underlying event to trace, if any
    ///</summary>
    IEvent? Event { get; }

    ///<summary>
    ///The recodrded event times
    ///</summary>
    List<double> Times { get; }

    ///<summary>
    ///Records the given time as an event time
    ///</summary>
    void Record(double time);

    ///<summary>
    ///Returns true if there is an event at the time in question.
    ///Checks in the range [time+tolerance; time+toleranceAcausal]
    ///</summary>
    bool SampleAt(double time, double tolerance = -1E-08, double toleranceAcausal = 1E-08);

    ///<summary>
    ///Returns the time of the last change (i.e. event) before the specified time.
    ///Returns double.NegativeInfinity if no prior change was found.
    ///</summary>
    double LastChanged(double time);

    ///<summary>
    ///Clears the event trace entirely.
    ///</summary>
    void Clear();

    ///<summary>
    ///Starts tracing the given event (if any), stops tracing previously traced event (if any).
    ///</summary>
    void Trace(IEvent? _event = null);
}
