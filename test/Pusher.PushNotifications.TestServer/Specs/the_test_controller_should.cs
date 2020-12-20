namespace Pusher.PushNotifications.TestServer.Specs
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;

    public class the_test_controller_should : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;
        private readonly Fixture _fixture = new();
        public the_test_controller_should(TestFixture testFixture) => _testFixture = testFixture;

        [Fact]
        public async Task return_a_token()
        {
            var userId = _fixture.Create<string>();

            var response = await _testFixture.Server
                .CreateHttpApiRequest<PushNotificationsController>(c => c.GenerateToken(userId))
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task send_to_interests()
        {
            var interest = _fixture.Create<string>();
            var body = _fixture.Create<string>();
            var message = new PublishMessage(interest, body);
                
            var response = await _testFixture.Server
                .CreateHttpApiRequest<PushNotificationsController>(c => c.PublishToInterests(message))
                .PostAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task send_to_users()
        {
            var userId = _fixture.Create<string>();
            var body = _fixture.Create<string>();
            var message = new PublishMessage(userId, body);
                
            var response = await _testFixture.Server
                .CreateHttpApiRequest<PushNotificationsController>(c => c.PublishToUsers(message))
                .PostAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task delete_a_user()
        {
            var userId = _fixture.Create<string>();

            var response = await _testFixture.Server
                .CreateHttpApiRequest<PushNotificationsController>(c => c.DeleteUser(userId))
                .SendAsync(HttpMethod.Delete.ToString());

            response.EnsureSuccessStatusCode();
        }
    }
}