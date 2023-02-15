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

    public T SampleAt(double time, bool after = true)
    {
        // get index of the event or get the next event after
        var indexOrNext = Times.BinarySearch(time);

        // There was no change at the current point in time -> clear case
        if(indexOrNext < 0)
        {
            // the element was not found -> binary complement holds the next larger hit or Times.Count
            indexOrNext = ~indexOrNext;
            
            // if the next larger index is 0, there was nothing before time
            if(indexOrNext == 0)
            {
                if(Times.Count>0)
                {
                    var prop = (after)? "at" : "before";
                    throw new IndexOutOfRangeException($"Tried to sample undefined trace signal {prop} time {time}, but the first sample is at {Times.First()}!");
                }
                else
                {
                    throw new IndexOutOfRangeException("Cannot sample an empty trace!");
                }
            }

            return Values[indexOrNext-1];
        }
        // there was at least one change at exactly this point in time -> find first / last value
        else
        {
            // seek the last change
            if(after)
            {
                while(indexOrNext < Times.Count && Times[indexOrNext]==time)
                {
                    indexOrNext++;
                }
                indexOrNext--;
            }
            // seek the value before the first change
            else
            {
                while(indexOrNext >= 0 && Times[indexOrNext]==time)
                {
                    indexOrNext--;
                }
                
                if(indexOrNext < 0)
                {
                    if(Times.Count>0)
                    {
                        var prop = (after)? "at" : "before";
                        throw new IndexOutOfRangeException($"Tried to sample undefined trace signal {prop} time {time}, but the first sample is at {Times.First()}!");
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("Cannot sample an empty trace!");
                    }
                }
            }
            return Values[indexOrNext];
        }
    }

    public double LastChanged(double time)
    {
        bool hasChangedAt(int index)
        {
            int idx = index;

            // seek the last value
            while(idx < Times.Count && Times[idx]==Times[index])
            {
                idx++;
            }
            idx--;
            var vLast = Values[idx];
            
            idx = index;
            // seek the first value before
            while(idx >= 0 && Times[idx]==Times[index])
            {
                idx--;
            }

            // value has changed, or this is the first event
            return (idx < 0) || !Values[idx].Equals(vLast);
        }

        // get index of the event or get the next event after
        var indexOrNext = Times.BinarySearch(time);

        // Hit the exact time of a change!
        if(indexOrNext >= 0)
        {
            // if we had a value change here, we are done!
            if(hasChangedAt(indexOrNext))
                return time;
        }
        else
        {
            // the element was not found -> binary complement holds the next larger hit or Times.Count
            indexOrNext = ~indexOrNext;
            indexOrNext--;
        }
        
        // go back until we find a time where the value changed
        while(indexOrNext >= 0)
        {
            // if there was a change at this point, return it!
            if(hasChangedAt(indexOrNext))
                return Times[indexOrNext];

            indexOrNext--;
        }

        // if we didn't find anything, there was no prior change
        return double.NegativeInfinity;
    }
}