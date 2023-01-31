namespace SystemCSharp;
using System.Collections;
public class SignalTrace<T> : ISignalTrace<T> where T : IEquatable<T>
{
    public string Name { get; protected set; }
    public ISignal<T>? Signal { get; protected set; } = null;
    public List<double> Times { get; protected set; }
    public List<T> Values { get; protected set; }

    public SignalTrace(string name, ISignal<T>? signal = null)
    {
        Name = name;
        Times = new();
        Values = new();

        if(signal != null)
            Trace(signal);
    }

    public void Trace(ISignal<T>? signal = null)
    {
        var oldSignal = Signal;
        Signal = signal;

        if (oldSignal != null && oldSignal != Signal)
        {
            oldSignal.Changed.StaticSensitivity -= AddFromSignal;
        }

        if (Signal != null && Signal != oldSignal)
        {
            Signal.Changed.StaticSensitivity += AddFromSignal;
        }

        // Initialize
        AddFromSignal();
    }

    private void AddFromSignal()
    {
        Add(Signal!.Changed.EventLoop.SimulationTime, Signal.Value);
    }

    public void Add(double time, T value)
    {
        Times.Add(time);
        Values.Add(value);
    }

    public void Clear()
    {
        Times.Clear();
        Values.Clear();
        if(Signal != null)
            Add(0.0, Signal.Initial);
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