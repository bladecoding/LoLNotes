//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Runtime.Serialization;
using FluorineFx.Util;

namespace FluorineFx.Json.Services
{
    /// <summary>
    /// Exception thrown for duplicate method names.
    /// </summary>
    [Serializable]
    public class DuplicateMethodException : ApplicationException
    {
        private const string _defaultMessage = "A method with the same name has been defined elsewhere on the service.";

        /// <summary>
        /// Initializes a new instance of the DuplicateMethodException class.
        /// </summary>
        public DuplicateMethodException() : this(null) { }
        /// <summary>
        /// Initializes a new instance of the DuplicateMethodException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>			
        public DuplicateMethodException(string message)
            : 
            base(StringUtils.MaskNullString(message, _defaultMessage)) {}
        /// <summary>
        /// Initializes a new instance of the DuplicateMethodException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception.</param>	
        /// <remarks>An exception that is thrown as a direct result of a previous exception should include a reference to the previous exception in the InnerException property. The InnerException property returns the same value that is passed into the constructor, or a null reference (Nothing in Visual Basic) if the InnerException property does not supply the inner exception value to the constructor.</remarks>			
        public DuplicateMethodException(string message, Exception innerException)
            :
            base(StringUtils.MaskNullString(message, _defaultMessage), innerException) { }
        /// <summary>
        /// Initializes a new instance of the DuplicateMethodException class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected DuplicateMethodException(SerializationInfo info, StreamingContext context)
            :
            base(info, context) {}
    }
}