namespace Pusher.PushNotifications.TestServer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public record PublishMessage(string To, string Title, string Body);

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
            var (to, title, body) = message;
            
            var interests = new List<string> {to};

            var notification = new Dictionary<string, object>
            {
                { "title", title },
                { "body", body }
            };

            var alert = new Dictionary<string, object>
            {
                { "title", title },
                { "body", body }
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
            var (to, title, body) = message;

            var users = new List<string> { to };

            var notification = new Dictionary<string, object>
            {
                { "title", title },
                { "body", body }
            };

            var alert = new Dictionary<string, object>
            {
                { "title", title },
                { "body", body }
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