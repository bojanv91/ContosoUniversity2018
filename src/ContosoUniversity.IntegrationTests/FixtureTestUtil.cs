using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Auth;
using ContosoUniversity.Services.Features.Users;
using Flurl.Http;

namespace ContosoUniversity.IntegrationTests
{
    public class FixtureTestUtil
    {
        private FlurlClient _api;

        public FixtureTestUtil(WebFixture fixture)
        {
            _api = fixture.FlurlClient;
        }

        public Task<CreateToken.Response> RegisterUserAndGetTokenAsync()
        {
            return RegisterUserAndGetTokenAsync("test_" + Guid.NewGuid());
        }

        public async Task<CreateToken.Response> RegisterUserAndGetTokenAsync(string username)
        {
            var response = await _api
                .Request("api/users/register")
                .PostJsonAsync(new RegisterUser.Request
                {
                    Fullname = username,
                    Email = username + "@example.com",
                    Password = "qwerty",
                    ConfirmPassword = "qwerty"
                })
                .ReceiveJson<RegisterUser.Response>();

            var tokenResponse = await _api
                .Request("api/auth/token")
                .PostJsonAsync(new CreateToken.Request
                {
                    Grant_type = "password",
                    Username = username + "@example.com",
                    Password = "qwerty"
                })
                .ReceiveJson<CreateToken.Response>();

            return tokenResponse;
        }
    }
}
