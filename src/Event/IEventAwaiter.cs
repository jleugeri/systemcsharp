using System.Runtime.CompilerServices;
namespace SystemCSharp;

public interface IEventAwaiter : INotifyCompletion
{
    bool IsCompleted { get; }

    void GetResult();
    new void OnCompleted(Action continuation);
}
