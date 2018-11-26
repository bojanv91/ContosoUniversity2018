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
    public class AuthTests
    {
        private readonly FlurlClient _api;
        private readonly FixtureTestUtil _util;

        public AuthTests(WebFixture fixture)
        {
            _api = fixture.FlurlClient;
            _util = fixture.Util;
        }

        [Fact]
        public async Task Create_token_with_valid_credential_will_return_status_OK()
        {
            // Arrange
            var randomName = "Test_" + Guid.NewGuid();
            var registerUserResponse = await _api.Request("api/users/register")
                .PostJsonAsync(new RegisterUser.Request
                {
                    Fullname = randomName,
                    Email = randomName + "@example.com",
                    Password = "qwerty",
                    ConfirmPassword = "qwerty"
                })
                .ReceiveJson<RegisterUser.Response>();

            // Act
            var tokenResponse = await _api.Request("api/auth/token")
                .PostJsonAsync(new CreateToken.Request
                {
                    Grant_type = "password",
                    Username = randomName + "@example.com",
                    Password = "qwerty"
                });

            // Assert
            tokenResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_token_with_invalid_credential_will_return_status_400()
        {
            //Act
            var tokenResponse = await _api.Request("api/auth/token")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CreateToken.Request
                {
                    Grant_type = "password",
                    Username = new Guid().ToString() + "@example.com",
                    Password = new Guid().ToString()
                });

            //Assert
            tokenResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
