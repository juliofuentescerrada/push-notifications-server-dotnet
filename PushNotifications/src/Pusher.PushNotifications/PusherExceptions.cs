namespace Pusher.PushNotifications
{
    using System;

    public sealed class PusherAuthException : Exception
    {
        public PusherAuthException(string message) : base(message) { }
    }

    public sealed class PusherTooManyRequestsException : Exception
    {
        public PusherTooManyRequestsException(string message) : base(message) { }
    }

    public sealed class PusherMissingInstanceException : Exception
    {
        public PusherMissingInstanceException(string message) : base(message) { }
    }

    public sealed class PusherValidationException : Exception
    {
        public PusherValidationException(string message) : base(message) { }
    }

    public sealed class PusherServerException : Exception
    {
        public PusherServerException(string message) : base(message) { }
    }
}