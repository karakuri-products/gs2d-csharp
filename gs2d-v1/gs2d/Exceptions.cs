using System;
using System.Collections.Generic;
using System.Text;

namespace gs2d
{
    public class ServoCommException : Exception
    {
        public ServoCommException() { }
        public ServoCommException(string message) : base(message) { }
        public ServoCommException(string message, Exception inner) : base(message, inner) { }
    }

    public class ReceiveDataTimeoutException : Exception
    {
        public ReceiveDataTimeoutException() { }
        public ReceiveDataTimeoutException(string message) : base(message) { }
        public ReceiveDataTimeoutException(string message, Exception inner) : base(message, inner) { }
    }

    public class BadInputParametersException : Exception
    {
        public BadInputParametersException() { }
        public BadInputParametersException(string message) : base(message) { }
        public BadInputParametersException(string message, Exception inner) : base(message, inner) { }
    }

    public class InvalidResponseDataException : Exception
    {
        public InvalidResponseDataException() { }
        public InvalidResponseDataException(string message) : base(message) { }
        public InvalidResponseDataException(string message, Exception inner) : base(message, inner) { }
    }

    public class NotSupportException : Exception
    {
        public NotSupportException() { }
        public NotSupportException(string message) : base(message) { }
        public NotSupportException(string message, Exception inner) : base(message, inner) { }
    }
}
