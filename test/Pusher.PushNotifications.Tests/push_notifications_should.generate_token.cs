namespace Pusher.PushNotifications.Tests
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using AutoFixture;
    using FluentAssertions;
    using Xunit;

    public partial class push_notifications_should
    {
        [Fact]
        public void generate_a_token()
        {
            var userId = _fixture.Create<string>();
            var token = _pushNotifications.GenerateToken(userId);

            token.Should().NotBeNullOrWhiteSpace();
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(token);
            securityToken.Claims.First(e => e.Type == "sub").Value.Should().Be(userId);
        }

        [Fact]
        public void fail_generating_a_token_with_an_invalid_user_id()
        {
            var userId = new string('u', 200);

            FluentActions
                .Invoking(() => _pushNotifications.GenerateToken(userId))
                .Should().Throw<ArgumentException>()
                .WithMessage($"UserId {userId} is too long (expected less than 165, got 200)");
        }
    }
}
