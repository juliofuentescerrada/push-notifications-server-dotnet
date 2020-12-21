namespace Pusher.PushNotifications
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.IdentityModel.Tokens;

    public sealed class PushNotifications
    {
        private const string ApplicationJsonMediaType = "application/json";
        private const string SdkVersion = "1.0.0";
        private const int InterestMaxLength = 64;
        private const int MaxRequestInterestsAllowed = 100;
        private const int MaxRequestUsersAllowed = 1000;
        private const int UserIdMaxLength = 164;

        private readonly string _instanceId;
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;

        public PushNotifications(HttpClient httpClient, PushNotificationsOptions options)
        {
            if (httpClient is null) throw new ArgumentNullException(nameof(httpClient));
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.InstanceId)) throw new ArgumentException(Resources.InvalidInstanceId, nameof(options));
            if (string.IsNullOrWhiteSpace(options.SecretKey)) throw new ArgumentException(Resources.InvalidSecretKey, nameof(options));

            _instanceId = options.InstanceId;
            _secretKey = options.SecretKey;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(string.Format(Resources.BaseUrl, _instanceId));
            _httpClient.DefaultRequestHeaders.Add("Accept", ApplicationJsonMediaType);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secretKey}");
            _httpClient.DefaultRequestHeaders.Add("X-Pusher-Library", $"pusher-push-notifications-server-dotnet {SdkVersion}");
        }

        public string GenerateToken(string userId)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));
            if (userId.Length > UserIdMaxLength) throw new ArgumentException(string.Format(Resources.InvalidUserId, userId, UserIdMaxLength + 1, userId.Length));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", userId) }),
                Issuer = string.Format(Resources.BaseUrl, _instanceId),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = credentials
            });

            return handler.WriteToken(token);
        }

        public Task<string> PublishToInterests(IReadOnlyCollection<string> interests, IDictionary<string, object> publishRequest)
        {
            if (interests is null) throw new ArgumentNullException(nameof(interests));
            if (publishRequest is null) throw new ArgumentNullException(nameof(publishRequest));
            if (!interests.Any()) throw new ArgumentException(Resources.InvalidEmptyInterests);
            if (interests.Count > MaxRequestInterestsAllowed) throw new ArgumentException(string.Format(Resources.InvalidMaxInterests, MaxRequestInterestsAllowed, interests.Count));

            var invalidInterest = interests.FirstOrDefault(i => i.Length > InterestMaxLength);
            if (invalidInterest != null) throw new ArgumentException(string.Format(Resources.InvalidInterest, invalidInterest, InterestMaxLength + 1, invalidInterest.Length));

            publishRequest.Add(nameof(interests), interests);

            return Publish(string.Format(Resources.PublishToInterestsEndpoint, _instanceId), publishRequest);
        }

        public Task<string> PublishToUsers(IReadOnlyCollection<string> users, IDictionary<string, object> publishRequest)
        {
            if (users is null) throw new ArgumentNullException(nameof(users));
            if (publishRequest is null) throw new ArgumentNullException(nameof(publishRequest));
            if (!users.Any()) throw new ArgumentException(Resources.InvalidEmptyUsers);
            if (users.Count > MaxRequestUsersAllowed) throw new ArgumentException(string.Format(Resources.InvalidMaxUsers, MaxRequestUsersAllowed, users.Count));

            var invalidUser = users.FirstOrDefault(i => i.Length > UserIdMaxLength);
            if (invalidUser != null) throw new ArgumentException(string.Format(Resources.InvalidUserId, invalidUser, UserIdMaxLength + 1, invalidUser.Length));

            publishRequest.Add(nameof(users), users);

            return Publish(string.Format(Resources.PublishToUsersEndpoint, _instanceId), publishRequest);
        }

        public async Task DeleteUser(string userId)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));
            if (userId.Length > UserIdMaxLength) throw new ArgumentException(string.Format(Resources.InvalidUserId, userId, UserIdMaxLength + 1, userId.Length));

            var response = await _httpClient.DeleteAsync(string.Format(Resources.DeleteUserEndpoint, _instanceId, HttpUtility.UrlEncode(userId)));
            var status = response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();

            CheckForServerErrors((int)status, body);
        }

        private async Task<string> Publish(string requestUri, IDictionary<string, object> request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, ApplicationJsonMediaType);
            var response = await _httpClient.PostAsync(requestUri, content);
            var status = response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();

            CheckForServerErrors((int)status, body);

            return JsonSerializer.Deserialize<PushNotificationResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.PublishId;
        }

        private static void CheckForServerErrors(int statusCode, string body)
        {
            string ExtractErrorDescription()
            {
                return JsonSerializer.Deserialize<PushNotificationErrorResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Description;
            }

            switch (statusCode)
            {
                case 401: throw new PusherAuthException(ExtractErrorDescription());
                case 404: throw new PusherMissingInstanceException(ExtractErrorDescription());
                case 409: throw new PusherTooManyRequestsException(ExtractErrorDescription());
                case int s when s >= 400 && s <= 499: throw new PusherValidationException(ExtractErrorDescription());
                case int s when s >= 500 && s <= 599: throw new PusherServerException(ExtractErrorDescription());
                default: return; // ok
            }
        }
    }
}
