namespace Nito.AsyncEx.Interop
{
    /// <summary>
    /// Arguments passed to a .NET event that follows the standard <c>sender, arguments</c> event pattern.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender of the event. This is commonly <see cref="object"/>.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments. This is commonly <see cref="EventArgs"/> or a derived type.</typeparam>
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct EventArguments<TSender, TEventArgs>
#pragma warning restore CA1815 // Override equals and operator equals on value types
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