using System;
using System.Runtime.Serialization;

namespace UGCS.TelemetryViewer
{
    [System.Serializable]
    public class ViewModelCreationException : Exception
    {
        public ViewModelCreationException(Type viewModelType, Exception e) : base($"Fail to create {viewModelType}", e) { }
        public ViewModelCreationException(Type viewModelType) : base($"Fail to create {viewModelType}") { }
        public ViewModelCreationException(string messge, Exception e) : base(messge, e) { }
        public ViewModelCreationException(string messge) : base(messge) { }

        protected ViewModelCreationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {

        }
    }
}
