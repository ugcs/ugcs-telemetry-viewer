using System;
using System.Runtime.Serialization;

namespace UGCS.TelemetryViewer
{
    [System.Serializable]
    public class NoViewModelBoundException : Exception
    {
        public NoViewModelBoundException(Type viewType) : base($"There is no bounded view model for {viewType}")
        {

        }

        protected NoViewModelBoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {

        }

    }
}
