namespace Pusher.PushNotifications.Tests
{
    using System;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Moq;
    using Moq.Protected;
    using Pusher.PushNotifications;
    using Xunit;

    public partial class push_notifications_should
    {
        private readonly string _instanceId;
        private readonly string _secretKey;
        private readonly PushNotifications _pushNotifications;
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<HttpMessageHandler> _mockHandler = new Mock<HttpMessageHandler>();
        private readonly Expression _anyCancellationToken = ItExpr.IsAny<CancellationToken>();

        public push_notifications_should()
        {
            _instanceId = _fixture.Create<string>();
            _secretKey = _fixture.Create<string>();

            var httpClient = SetupHttpClient();
            var options = new PushNotificationsOptions { InstanceId = _instanceId, SecretKey = _secretKey };
            _pushNotifications = new PushNotifications(httpClient, options);
        }

        [Fact]
        public void fail_being_instantiated_without_options()
        {
            FluentActions
                .Invoking(() => new PushNotifications(new HttpClient(), null))
                .Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("options");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void fail_being_instantiated_without_a_valid_instance_id(string instanceId)
        {
            var options = new PushNotificationsOptions
            {
                InstanceId = instanceId,
                SecretKey = _fixture.Create<string>()
            };

            FluentActions
                .Invoking(() => new PushNotifications(new HttpClient(), options))
                .Should().Throw<ArgumentException>()
                .Which.ParamName.Should().Be("options");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void fail_being_instantiated_without_a_valid_secret_key(string secretKey)
        {
            var options = new PushNotificationsOptions
            {
                InstanceId = _fixture.Create<string>(),
                SecretKey = secretKey
            };

            FluentActions
                .Invoking(() => new PushNotifications(new HttpClient(), options))
                .Should().Throw<ArgumentException>()
                .Which.ParamName.Should().Be("options");
        }

        [Fact]
        public void fail_being_instantiated_without_an_http_client()
        {
            var options = new PushNotificationsOptions
            {
                InstanceId = _fixture.Create<string>(),
                SecretKey = _fixture.Create<string>()
            };

            FluentActions
                .Invoking(() => new PushNotifications(null, options))
                .Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("httpClient");
        }

        private HttpClient SetupHttpClient()
        {
            var mockContent = new StringContent(JsonSerializer.Serialize(new PushNotificationResponse { PublishId = _fixture.Create<string>() }));
            var mockResponse = new HttpResponseMessage { Content = mockContent };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(nameof(HttpClient.SendAsync), ItExpr.IsAny<HttpRequestMessage>(), _anyCancellationToken)
                .ReturnsAsync(mockResponse);

            return new HttpClient(_mockHandler.Object);
        }
    }
}