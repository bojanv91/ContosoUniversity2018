using System;
using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Auth;
using ContosoUniversity.Services.Features.Users;
using Flurl.Http;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Features
{
    [Collection("WebCollection")]
    public class UserTests
    {
        private readonly FlurlClient _api;
        private readonly FixtureTestUtil _util;

        public UserTests(WebFixture fixture)
        {
            _api = fixture.FlurlClient;
            _util = fixture.Util;
        }

        [Fact]
        public async Task Query_users()
        {
            // Arrange
            var fullname = "Pece_" + Guid.NewGuid();
            var tokenResponse = await _util.RegisterUserAndGetTokenAsync(fullname);

            //Act
            var response = await _api.Request("api/users")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .GetJsonAsync<QueryUsers.Response>();

            //Assert
            response.Items.ShouldContain(x => x.Fullname == fullname);
        }
    }
}
