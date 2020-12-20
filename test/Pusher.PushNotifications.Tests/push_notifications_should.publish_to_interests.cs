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
        public async Task publish_to_interests()
        {
            var interests = _fixture.Create<List<string>>();
            var publishRequest = _fixture.Create<Dictionary<string, object>>();

            await _pushNotifications.PublishToInterests(interests, publishRequest);

            _mockHandler.Protected().Verify(nameof(HttpClient.SendAsync), Times.AtLeastOnce(), ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.Headers.Any(e => e.Key == "Authorization" && e.Value.Contains($"Bearer {_secretKey}")) &&
                    request.RequestUri == new Uri($"https://{_instanceId}.pushnotifications.pusher.com/publish_api/v1/instances/{_instanceId}/publishes/interests")),
                _anyCancellationToken);
        }

        [Fact]
        public void fail_publishing_to_interests_with_a_null_publish_request()
        {
            FluentActions
                .Invoking(() => _pushNotifications.PublishToUsers(new List<string>(), null))
                .Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("publishRequest");
        }

        [Fact]
        public void fail_publishing_to_interests_with_a_null_interests_list()
        {
            FluentActions
                .Invoking(() => _pushNotifications.PublishToInterests(null, new Dictionary<string, object>()))
                .Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("interests");
        }

        [Fact]
        public void fail_publishing_to_interests_with_an_empty_interests_list()
        {
            FluentActions
                .Invoking(() => _pushNotifications.PublishToInterests(new List<string>(), new Dictionary<string, object>()))
                .Should().Throw<ArgumentException>()
                .WithMessage("Publish method expects at least one interest");
        }

        [Fact]
        public void fail_publishing_to_interests_with_too_many_interests()
        {
            var interests = Enumerable.Range(0, 101).Select(i => _fixture.Create<string>()).ToList();

            FluentActions
                .Invoking(() => _pushNotifications.PublishToInterests(interests, new Dictionary<string, object>()))
                .Should().Throw<ArgumentException>()
                .WithMessage("Publish requests can only have up to 100 interests (given 101)");
        }

        [Fact]
        public void fail_publishing_to_interests_with_invalid_interests()
        {
            var interest = new string('a', 200);
            var interests = new List<string> { interest };

            FluentActions
                .Invoking(() => _pushNotifications.PublishToInterests(interests, new Dictionary<string, object>()))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Interest {interest} is too long (expected less than 65, got 200)");
        }
    }
}