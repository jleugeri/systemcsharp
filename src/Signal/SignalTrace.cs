namespace SystemCSharp;
using System.Collections;
using Serilog;

public class SignalTrace<T> : ISignalTrace<T> where T : IEquatable<T>
{
    public string Name { get; protected set; }
    public ISignal<T>? Signal { get; protected set; }
    public List<double> Times { get; protected set; }
    public List<T> Values { get; protected set; }
    public bool TraceAllUpdates { get; protected set; }

    public SignalTrace(string name, ISignal<T>? signal = null, bool traceAllUpdates = true)
    {
        Name = name;
        Times = new();
        Values = new();
        Signal = null;
        TraceAllUpdates = traceAllUpdates;

        Trace(signal);

    }

    public void Trace(ISignal<T>? signal = null)
    {
        var oldSignal = Signal;
        Signal = signal;

        if (oldSignal != null && oldSignal != Signal)
        {
            if (TraceAllUpdates)
            {
                oldSignal.Updated.StaticSensitivity -= RecordSignal;
            }
            else
            {
                oldSignal.Changed.StaticSensitivity -= RecordSignal;
            }
        }

        if (Signal != null && Signal != oldSignal)
        {
            if (TraceAllUpdates)
            {
                Signal.Updated.StaticSensitivity += RecordSignal;
            }
            else
            {
                Signal.Changed.StaticSensitivity += RecordSignal;
            }

            // Initialize
            RecordSignal();
        }

    }

    public void RecordSignal()
    {
        if (Signal != null)
        {
            Log.Logger.Verbose("Trace '{name}': Recording value {value} at time {time}.", Name, Signal.Value, Signal!.Updated.EventLoop.SimulationTime);
            Record(Signal.Updated.EventLoop.SimulationTime, Signal.Value);
        }
    }

    public void Record(double time, T value)
    {
        Times.Add(time);
        Values.Add(value);
    }

    public void Clear()
    {
        Times.Clear();
        Values.Clear();
        if (Signal != null)
            RecordSignal();
    }

    public IEnumerator<(double Time, T Value)> GetEnumerator()
    {
        return Enumerable.Zip(Times, Values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}