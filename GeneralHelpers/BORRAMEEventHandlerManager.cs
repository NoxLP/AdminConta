using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

namespace AdConta
{

    public class CustomCurrentChangingEventManager : WeakEventManager
    {
        private CustomCurrentChangingEventManager()
        {
            
        }

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(CollectionViewSource source,
                                      EventHandler<CurrentChangingEventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (handler == null)
                throw new ArgumentNullException("handler");
            
            CurrentManager.ProtectedAddHandler(source, handler);
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(CollectionViewSource source,
                                         EventHandler<CurrentChangingEventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static CustomCurrentChangingEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(CustomCurrentChangingEventManager);
                CustomCurrentChangingEventManager manager =
                    (CustomCurrentChangingEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new CustomCurrentChangingEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }



        /// <summary>
        /// Return a new list to hold listeners to the event.
        /// </summary>
        protected override ListenerList NewListenerList()
        {
            return new ListenerList<CurrentChangingEventArgs>();
        }


        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        protected override void StartListening(object source)
        {
            ICollectionView typedSource = CollectionViewSource.GetDefaultView(source);
            typedSource.CurrentChanging += new CurrentChangingEventHandler(OnCurrentChanging);
        }

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        protected override void StopListening(object source)
        {
            ICollectionView typedSource = CollectionViewSource.GetDefaultView(source);
            typedSource.CurrentChanging -= new CurrentChangingEventHandler(OnCurrentChanging);
        }

        /// <summary>
        /// Event handler for the CurrentChanging event.
        /// </summary>
        void OnCurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            DeliverEvent(sender, e);
        }
    }
    /// <summary>
    /// Make weak reference event
    /// http://stackoverflow.com/questions/1747235/weak-event-handler-model-for-use-with-lambdas
    /// </summary>
    public sealed class EventHandlerManager
    {
        //This overload handles any type of EventHandler
        public static void SetAnyHandler<S, TDelegate, TArgs>(
            Func<EventHandler<TArgs>, TDelegate> converter,
            Action<TDelegate> add, 
            Action<TDelegate> remove,
            S subscriber, 
            Action<S, TArgs> action)
            where TArgs : EventArgs
            where TDelegate : class
            where S : class
        {
            var subs_weak_ref = new WeakReference(subscriber);
            TDelegate handler = null;
            handler = converter(new EventHandler<TArgs>(
                (s, e) =>
                {
                    var subs_strong_ref = subs_weak_ref.Target as S;
                    if (subs_strong_ref != null)
                    {
                        action(subs_strong_ref, e);
                    }
                    else
                    {
                        remove(handler);
                        handler = null;
                    }
                }));
            add(handler);
        }

        // this overload is simplified for generic EventHandlers
        public static void SetAnyHandler<S, TArgs>(
            Action<EventHandler<TArgs>> add, 
            Action<EventHandler<TArgs>> remove,
            S subscriber, 
            Action<S, TArgs> action)
            where TArgs : EventArgs
            where S : class
        {
            SetAnyHandler<S, EventHandler<TArgs>, TArgs>(
                h => h, add, remove, subscriber, action);
        }
    }
}
