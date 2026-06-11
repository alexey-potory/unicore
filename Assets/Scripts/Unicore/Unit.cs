namespace Unicore
{
    /// <summary>
    /// Represents absence of meaningful value.
    /// </summary>
    public readonly struct Unit
    {
        /// <summary>
        /// Gets canonical unit value.
        /// </summary>
        /// <value>A singleton-like unit value.</value>
        public static Unit Value { get; } = new();
    }
}
