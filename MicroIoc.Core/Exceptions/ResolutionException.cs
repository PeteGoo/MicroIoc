using System;

namespace MicroIoc
{
    public class ResolutionException : Exception
    {
        public ResolutionException(string message)
            : base(message) { }

        public ResolutionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}