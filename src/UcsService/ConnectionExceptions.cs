using System;
using System.Runtime.Serialization;

namespace UGCS.UcsServices
{
    [System.Serializable]
    public class ConnectionException : Exception
    {
        public ConnectionException(string message)
            : base(message)
        {

        }

        public ConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ConnectionException(Exception innerException)
            : base("Connection failure.", innerException)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {

        }
    }
}
