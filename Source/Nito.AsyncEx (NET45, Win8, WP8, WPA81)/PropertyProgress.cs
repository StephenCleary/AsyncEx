using System;
using System.ComponentModel;
using System.Threading;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A progress implementation that stores progress updates in a property. If this instance is created on a UI thread, its <see cref="Progress"/> property is suitable for data binding.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    public sealed class PropertyProgress<T> : IProgress<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// The context of the thread that created this instance.
        /// </summary>
        private readonly SynchronizationContext _context;

        /// <summary>
        /// The last reported progress value.
        /// </summary>
        private T _progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProgress&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="initialProgress">The initial progress value.</param>
        public PropertyProgress(T initialProgress)
        {
            _context = SynchronizationContextHelpers.CurrentOrDefault;
            _progress = initialProgress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProgress&lt;T&gt;"/> class.
        /// </summary>
        public PropertyProgress()
            : this(default(T))
        {
        }

        /// <summary>
        /// The last reported progress value.
        /// </summary>
        public T Progress
        {
            get
            {
                return _progress;
            }

            private set
            {
                _progress = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Progress"));
                }
            }
        }

        void IProgress<T>.Report(T value)
        {
            _context.Post(_ => { Progress = value; }, null);
        }

        /// <summary>
        /// Occurs when the property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
