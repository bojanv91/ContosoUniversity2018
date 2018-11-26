using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ContosoUniversity.Infrastructure;
using ContosoUniversity.Infrastructure.Behaviors;
using ContosoUniversity.Infrastructure.Services;
using ContosoUniversity.Services.Features.Users;
using ContosoUniversity.Web.Filters;
using FluentValidation.AspNetCore;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;

namespace ContosoUniversity.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMediatR(typeof(RegisterUser), typeof(MartenMapping))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(DocumentSessionBehavior<,>));

            services.AddSingleton<IDocumentStore>(x => MartenFactory.CreateStore(Configuration["ConnectionString"]));
            services.AddScoped<IDocumentSession>(x => x.GetService<IDocumentStore>().OpenSession());

            services
                .AddMvc(options =>
                {
                    options.Filters.Add(typeof(ErrorHandlingFilter));
                    options.Filters.Add(new AuthorizeFilter());
                })
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<RegisterUser>();
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            services
                .AddHttpContextAccessor()
                .AddRouting(options => options.LowercaseUrls = true);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                        ValidateLifetime = true
                    };
                });

            services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Info { Title = "ContosoUniversity", Version = "v1" });
                    options.CustomSchemaIds(type => type.FullName);
                    options.DescribeAllEnumsAsStrings();
                    options.DescribeAllParametersInCamelCase();
                    options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please enter JWT with Bearer into field",
                        Name = "Authorization",
                        Type = "access_token"
                    });
                    options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                        { "Bearer", Enumerable.Empty<string>() },
                    });

                    // Set FluentValidation support for Swagger JSON and UI (by default it uses DataAnnotation).
                    options.SchemaFilter<FluentValidationSwaggerFilter>();

                    // Set the comments path for the Swagger JSON and UI.
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app
                .UseHsts()
                .UseHttpsRedirection()
                .UseSwagger(options =>
                {
                    options.PreSerializeFilters.Add((document, request) =>
                    {
                        // Support [controller] route templates to be displayed as lowercase. See more: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/74
                        document.Paths = document.Paths.ToDictionary(p => p.Key.ToLowerInvariant(), p => p.Value);
                    });

                })
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ContosoUniversity");
                    options.RoutePrefix = "docs";
                })
                .UseAuthentication()
                .UseMvc();
        }
    }
}
