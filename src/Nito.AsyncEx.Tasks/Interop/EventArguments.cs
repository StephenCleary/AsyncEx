namespace Nito.AsyncEx.Interop
{
    /// <summary>
    /// Arguments passed to a .NET event that follows the standard <c>sender, arguments</c> event pattern.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender of the event. This is commonly <see cref="object"/>.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments. This is commonly <see cref="EventArgs"/> or a derived type.</typeparam>
    public struct EventArguments<TSender, TEventArgs>
    {
        /// <summary>
        /// The sender of the event.
        /// </summary>
        public TSender Sender { get; set; }

        /// <summary>
        /// The event arguments.
        /// </summary>
        public TEventArgs EventArgs { get; set; }
    }
}