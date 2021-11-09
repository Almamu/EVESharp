using System;

namespace EVESharp.Node
{
    /// <summary>
    /// Timer entry information
    /// </summary>
    class Timer
    {
        /// <summary>
        /// The timestamp when the timer should be fired
        /// </summary>
        public long DateTime;
        /// <summary>
        /// The method to call
        /// </summary>
        public Action<int> Callback;
        /// <summary>
        /// The parameter to pass onto the <see cref="Callback"/>
        /// </summary>
        public int CallbackParameter;
    }
}