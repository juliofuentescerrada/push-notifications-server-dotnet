namespace Pusher.PushNotifications.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public partial class push_notifications_should
    {
        [Fact]
        public async Task publish_to_users()
        {
            var users = _fixture.Create<List<string>>();
            var publishRequest = _fixture.Create<Dictionary<string, object>>();

            await _pushNotifications.PublishToUsers(users, publishRequest);

            _mockHandler.Protected().Verify(nameof(HttpClient.SendAsync), Times.AtLeastOnce(), ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.Headers.Any(e => e.Key == "Authorization" && e.Value.Contains($"Bearer {_secretKey}")) &&
                    request.RequestUri == new Uri($"https://{_instanceId}.pushnotifications.pusher.com/publish_api/v1/instances/{_instanceId}/publishes/users")),
                _anyCancellationToken);
        }

        [Fact]
        public void fail_publishing_to_users_with_a_null_publish_request()
        {
            FluentActions
                .Invoking(() => _pushNotifications.PublishToUsers(new List<string>(), null))
                .Should().Throw<ArgumentNullException>()
                .Where(e => e.ParamName == "publishRequest");
        }

        [Fact]
        public void fail_publishing_to_users_with_a_null_users_list()
        {
            FluentActions
                .Invoking(() => _pushNotifications.PublishToUsers(null, new Dictionary<string, object>()))
                .Should().Throw<ArgumentNullException>()
                .Where(e => e.ParamName == "users");
        }

        [Fact]
        public void fail_publishing_to_users_with_an_empty_users_list()
        {
            FluentActions
                .Invoking(() => _pushNotifications.PublishToUsers(new List<string>(), new Dictionary<string, object>()))
                .Should().Throw<ArgumentException>()
                .WithMessage("Publish method expects at least one user");
        }

        [Fact]
        public void fail_publishing_to_users_with_too_many_users()
        {
            var users = Enumerable.Range(0, 1001).Select(i => _fixture.Create<string>()).ToList();

            FluentActions
                .Invoking(() => _pushNotifications.PublishToUsers(users, new Dictionary<string, object>()))
                .Should().Throw<ArgumentException>()
                .WithMessage("Publish requests can only have up to 1000 users (given 1001)");
        }

        [Fact]
        public void fail_publishing_to_users_with_invalid_users()
        {
            var userId = new string('u', 200);
            var users = new List<string> { userId };

            FluentActions
                .Invoking(() => _pushNotifications.PublishToUsers(users, new Dictionary<string, object>()))
                .Should().Throw<ArgumentException>()
                .WithMessage($"User id {userId} is too long (expected less than 165, got 200)");
        }
    }
}