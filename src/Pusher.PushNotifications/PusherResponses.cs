namespace Pusher.PushNotifications
{
    public sealed class PushNotificationResponse
    {
        public string PublishId { get; set; }
    }

    public sealed class PushNotificationErrorResponse
    {
        public string Error { get; set; }
        public string Description { get; set; }
    }
}