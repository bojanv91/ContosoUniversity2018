using Flurl.Http;
using ContosoUniversity.Web;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;

namespace ContosoUniversity.IntegrationTests
{
    public class WebFixture : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _webAppFactory;

        public WebFixture()
        {
            _webAppFactory = new TestWebApplicationFactory();
            FlurlClient = new FlurlClient(_webAppFactory.CreateClient());

            var store = (IDocumentStore)_webAppFactory.Server.Host.Services.GetService(typeof(IDocumentStore));
            store.Advanced.Clean.DeleteAllDocuments();

            Util = new FixtureTestUtil(this);
        }

        public FlurlClient FlurlClient { get; private set; }

        public FixtureTestUtil Util { get; private set; }

        public void Dispose()
        {
            FlurlClient.Dispose();
            _webAppFactory.Dispose();
        }
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Use the existing web application configuration.
            base.ConfigureWebHost(builder);

            // Apply modifications specific for the integration tests project.
            builder
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile($"appsettings.IntegrationTests.json", optional: true, reloadOnChange: true);
                });
        }
    }
}
