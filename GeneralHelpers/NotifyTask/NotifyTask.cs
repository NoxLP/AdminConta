using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AdConta
{
    //https://github.com/StephenCleary/Mvvm.Async
    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    public sealed class NotifyTask : INotifyPropertyChanged, INotifyTask
    {
        /// <summary>
        /// Initializes a task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        private NotifyTask(Task task, Action<INotifyTask> doWhenCompleted)
        {
            Task = task;
            TaskCompleted = MonitorTaskAsync(task, doWhenCompleted);
        }

        private async Task MonitorTaskAsync(Task task, Action<INotifyTask> doWhenCompleted)
        {
            try
            {
                await task;
            }
            catch
            {
            }
            finally
            {
                NotifyProperties(task);
                if (doWhenCompleted != null && IsSuccessfullyCompleted)
                    doWhenCompleted(this);
            }
        }

        private void NotifyProperties(Task task)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;

            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs("Status"));
                propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs("Exception"));
                propertyChanged(this, new PropertyChangedEventArgs("InnerException"));
                propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
                propertyChanged(this, new PropertyChangedEventArgs("Status"));
                propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs("Status"));
                propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
            }
            propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
            propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
        }

        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled). This property never changes and is never <c>null</c>.
        /// </summary>
        public Task TaskCompleted { get; private set; }

        /// <summary>
        /// Gets the current task status. This property raises a notification when the task completes.
        /// </summary>
        public TaskStatus Status { get { return Task.Status; } }

        /// <summary>
        /// Gets whether the task has completed. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        public bool IsCompleted { get { return Task.IsCompleted; } }

        /// <summary>
        /// Gets whether the task is busy (not completed). This property raises a notification when the value changes to <c>false</c>.
        /// </summary>
        public bool IsNotCompleted { get { return !Task.IsCompleted; } }

        /// <summary>
        /// Gets whether the task has completed successfully. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }

        /// <summary>
        /// Gets whether the task has been canceled. This property raises a notification only if the task is canceled (i.e., if the value changes to <c>true</c>).
        /// </summary>
        public bool IsCanceled { get { return Task.IsCanceled; } }

        /// <summary>
        /// Gets whether the task has faulted. This property raises a notification only if the task faults (i.e., if the value changes to <c>true</c>).
        /// </summary>
        public bool IsFaulted { get { return Task.IsFaulted; } }

        /// <summary>
        /// Gets the wrapped faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        public AggregateException Exception { get { return Task.Exception; } }

        /// <summary>
        /// Gets the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        public Exception InnerException { get { return (Exception == null) ? null : Exception.InnerException; } }

        /// <summary>
        /// Gets the error message for the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        public string ErrorMessage { get { return (InnerException == null) ? null : InnerException.Message; } }

        /// <summary>
        /// Event that notifies listeners of property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        /// <param name="doWhenCompleted">Action<INotifyTask> to execute when task complete.</param>
        public static NotifyTask Create(Task task, Action<INotifyTask> doWhenCompleted = null)
        {
            return new NotifyTask(task, doWhenCompleted);
        }

        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="task">The task to watch.</param>
        /// <param name="defaultResult">The default "result" value for the task while it is not yet complete.</param>
        /// <param name="doWhenCompleted">Action<INotifyTask> to execute when task complete.</param>
        public static NotifyTask<TResult> Create<TResult>(Task<TResult> task, Action<INotifyTask> doWhenCompleted = null, TResult defaultResult = default(TResult))
        {
            return new NotifyTask<TResult>(task, defaultResult, doWhenCompleted);
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction<INotifyTask>">The asynchronous code to execute.</param>
        /// <param name="doWhenCompleted">Action<INotifyTask> to execute when task complete.</param>
        public static NotifyTask Create(Func<Task> asyncAction, Action<INotifyTask> doWhenCompleted = null)
        {
            return Create(asyncAction(), doWhenCompleted);
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction<INotifyTask>">The asynchronous code to execute.</param>
        /// <param name="defaultResult">The default "result" value for the task while it is not yet complete.</param>
        /// <param name="doWhenCompleted">Action<INotifyTask> to execute when task complete.</param>
        public static NotifyTask<TResult> Create<TResult>(Func<Task<TResult>> asyncAction, Action<INotifyTask> doWhenCompleted = null, TResult defaultResult = default(TResult))
        {
            return Create(asyncAction(), doWhenCompleted, defaultResult);
        }
    }

    //public sealed class NotifyTask : INotifyPropertyChanged
    //{
    //    public NotifyTask(Task task)
    //    {
    //        Task = task;
    //        TaskCompleted = MonitorTaskAsync(task);
    //    }
    //    public NotifyTask(Task task, DispatcherObject control, DispatcherPriority priority)
    //    {
    //        Task = task;
    //        if (!task.IsCompleted)
    //        {
    //            var _ = DispatcherWatchTaskAsync(task, control, priority);
    //        }
    //    }

    //    private async Task WatchTaskAsync(Task task)
    //    {
    //        try
    //        {
    //            await task;
    //        }
    //        catch
    //        {
    //        }
    //        finally
    //        {
    //            SetProperties(task);
    //        }
    //    }
    //    private async Task DispatcherWatchTaskAsync(Task task, DispatcherObject control, DispatcherPriority priority)
    //    {
    //        try
    //        {
    //            await control.Dispatcher.InvokeAsync(() => task, priority);
    //        }
    //        catch
    //        {
    //        }
    //        finally
    //        {
    //            SetProperties(task);
    //        }
    //    }
    //    private void SetProperties(Task task)
    //    {
    //        var propertyChanged = PropertyChanged;
    //        if (propertyChanged == null)
    //            return;
    //        propertyChanged(this, new PropertyChangedEventArgs("Status"));
    //        propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
    //        propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
    //        if (task.IsCanceled)
    //        {
    //            propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
    //        }
    //        else if (task.IsFaulted)
    //        {
    //            propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
    //            propertyChanged(this, new PropertyChangedEventArgs("Exception"));
    //            propertyChanged(this,
    //              new PropertyChangedEventArgs("InnerException"));
    //            propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
    //        }
    //        else
    //        {
    //            propertyChanged(this,
    //              new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
    //            propertyChanged(this, new PropertyChangedEventArgs("Result"));
    //        }
    //    }
    //    public static NotifyTask NewAndGetNotifyTask(Task task)
    //    {
    //        NotifyTask ntc = new NotifyTask(task);
    //        return ntc;
    //    }

    //    #region properties
    //    public Task Task { get; private set; }
    //    public TaskStatus Status { get { return Task.Status; } }
    //    public bool IsCompleted { get { return Task.IsCompleted; } }
    //    public bool IsNotCompleted { get { return !Task.IsCompleted; } }
    //    public bool IsSuccessfullyCompleted
    //    {
    //        get
    //        {
    //            return Task.Status == TaskStatus.RanToCompletion;
    //        }
    //    }
    //    public bool IsCanceled { get { return Task.IsCanceled; } }
    //    public bool IsFaulted { get { return Task.IsFaulted; } }
    //    public AggregateException Exception { get { return Task.Exception; } }
    //    public Exception InnerException
    //    {
    //        get
    //        {
    //            return (Exception == null) ? null : Exception.InnerException;
    //        }
    //    }
    //    public string ErrorMessage
    //    {
    //        get
    //        {
    //            return (InnerException == null) ? null : InnerException.Message;
    //        }
    //    }
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    #endregion
    //}
}
