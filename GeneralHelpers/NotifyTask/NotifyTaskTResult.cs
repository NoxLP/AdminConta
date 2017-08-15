using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AdConta
{
    //https://github.com/StephenCleary/Mvvm.Async

    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    public sealed class NotifyTask<TResult> : INotifyPropertyChanged, INotifyTask
    {
        /// <summary>
        /// The "result" of the task when it has not yet completed.
        /// </summary>
        private readonly TResult _defaultResult;

        /// <summary>
        /// Initializes a task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        /// <param name="defaultResult">The value to return from <see cref="Result"/> while the task is not yet complete.</param>
        internal NotifyTask(Task<TResult> task, TResult defaultResult, Action<INotifyTask> doWhenCompleted)
        {
            _defaultResult = defaultResult;
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
                propertyChanged(this, new PropertyChangedEventArgs("Result"));
                propertyChanged(this, new PropertyChangedEventArgs("Status"));
                propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
            }
            propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
            propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
        }

        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        public Task<TResult> Task { get; private set; }

        /// <summary>
        /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled). This property never changes and is never <c>null</c>.
        /// </summary>
        public Task TaskCompleted { get; private set; }

        /// <summary>
        /// Gets the result of the task. Returns the "default result" value specified in the constructor if the task has not yet completed successfully. This property raises a notification when the task completes successfully.
        /// </summary>
        public TResult Result { get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : _defaultResult; } }

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
    }

    ////Bind async task
    ////https://msdn.microsoft.com/en-us/magazine/dn605875.aspx?f=255&MSPPError=-2147217396
    //public sealed class NotifyTask<TResult> : INotifyPropertyChanged
    //{
    //    public NotifyTask(Task<TResult> task)
    //    {
    //        Task = task;
    //        if (!task.IsCompleted)
    //        {
    //            var _ = WatchTaskAsync(task);
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
    //    #region properties
    //    public Task<TResult> Task { get; private set; }
    //    public TResult Result
    //    {
    //        get
    //        {
    //            return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);
    //        }
    //    }
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
