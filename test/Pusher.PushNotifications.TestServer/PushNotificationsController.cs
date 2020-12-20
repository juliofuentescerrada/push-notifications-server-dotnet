namespace Pusher.PushNotifications.TestServer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public sealed class PublishMessage
    {
        public string To { get; }
        public string Body { get; }

        public PublishMessage(string to, string body)
        {
            To = to;
            Body = body;
        }
    };
    
    [Route("push-notifications")]
    public class PushNotificationsController : Controller
    {
        private readonly PushNotifications _pushNotifications;

        public PushNotificationsController(PushNotifications pushNotifications)
        {
            _pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
        }

        [HttpGet("token")]
        public ActionResult GenerateToken([FromQuery] string user_id)
        {
            var token = _pushNotifications.GenerateToken(user_id);
            return Json(new { token });
        }

        [HttpPost("interests")]
        public async Task<ActionResult<string>> PublishToInterests([FromBody] PublishMessage message)
        {
            var interests = new List<string> { message.To };

            var notification = new Dictionary<string, object>
            {
                { "title", $"Hello android {message.To} fans!" },
                { "body", message.Body }
            };

            var alert = new Dictionary<string, object>
            {
                { "title", $"Hello iOS {message.To} fans!" },
                { "body", message.Body }
            };

            var publishRequest = new Dictionary<string, object>
            {
                { "fcm",  new { notification } },
                { "apns",  new { aps = new { alert } }}
            };

            var publishId = await _pushNotifications.PublishToInterests(interests, publishRequest);
            return Ok(publishId);
        }

        [HttpPost("users")]
        public async Task<ActionResult<string>> PublishToUsers([FromBody] PublishMessage message)
        {
            var users = new List<string> { message.To };

            var notification = new Dictionary<string, object>
            {
                { "title", $"Hello {message.To}!" },
                { "body", message.Body }
            };

            var alert = new Dictionary<string, object>
            {
                { "title", $"Hello {message.To}!" },
                { "body", message.Body }
            };

            var publishRequest = new Dictionary<string, object>
            {
                { "fcm",  new { notification } },
                { "apns",  new { aps = new { alert } }}
            };

            var publishId = await _pushNotifications.PublishToUsers(users, publishRequest);
            return Ok(publishId);
        }

        [HttpDelete("user/{userId}")]
        public async Task<ActionResult> DeleteUser([FromRoute] string userId)
        {
            await _pushNotifications.DeleteUser(userId);
            return Ok();
        }
    }
}