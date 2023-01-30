using System.Runtime.CompilerServices;

public interface IEventAwaiter : INotifyCompletion
{
    bool IsCompleted { get; }

    void GetResult();
    new void OnCompleted(Action continuation);
}
