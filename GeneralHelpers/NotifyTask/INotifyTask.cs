using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AdConta
{
    public interface INotifyTask
    {
        string ErrorMessage { get; }
        AggregateException Exception { get; }
        Exception InnerException { get; }
        bool IsCanceled { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        bool IsNotCompleted { get; }
        bool IsSuccessfullyCompleted { get; }
        TaskStatus Status { get; }
        Task TaskCompleted { get; }

        event PropertyChangedEventHandler PropertyChanged;
    }
}