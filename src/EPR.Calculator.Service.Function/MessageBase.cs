using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// Represents the base class for all message types.
    /// </summary>
    public abstract class MessageBase
    {
        /// <summary>
        /// Gets or sets the type identifier of the message.
        /// This value is typically used to determine the specific message subclass during deserialization.
        /// This property may be <c>null</c> if the message type is not specified.
        /// </summary>
        public string? MessageType { get; set; }
    }
}
