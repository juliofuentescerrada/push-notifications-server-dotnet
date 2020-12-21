namespace Pusher.PushNotifications.Tests
{
    using System;
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
        public async Task delete_a_user()
        {
            var userId = _fixture.Create<string>();

            await _pushNotifications.DeleteUser(userId);

            _mockHandler.Protected().Verify(nameof(HttpClient.SendAsync), Times.AtLeastOnce(), ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Delete &&
                    request.Headers.Any(e => e.Key == "Authorization" && e.Value.Contains($"Bearer {_secretKey}")) &&
                    request.RequestUri == new Uri($"https://{_instanceId}.pushnotifications.pusher.com/customer_api/v1/instances/{_instanceId}/users/{userId}")),
                _anyCancellationToken);
        }

        [Fact]
        public void fail_deleting_a_user_with_a_null_user_id()
        {
            FluentActions
                .Invoking(() => _pushNotifications.DeleteUser(null))
                .Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("userId");
        }

        [Fact]
        public void fail_deleting_a_user_with_an_invalid_user_id()
        {
            var userId = new string('u', 200);

            FluentActions
                .Invoking(() => _pushNotifications.DeleteUser(userId))
                .Should().Throw<ArgumentException>()
                .WithMessage($"UserId {userId} is too long (expected less than 165, got 200)");
        }
    }
}