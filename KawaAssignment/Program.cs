using Application;
using Infrastructure;
using Presistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Prometheus;


namespace KawaAssignment
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration setup
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true) // Load Development settings
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = Configuration["Keycloack:Authority"];
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    //ValidAudience = Configuration["Keycloack:Audience"],
                    ValidAudiences = new List<string>
                        {
                            Configuration["Keycloack:AudienceAccount"],
                            Configuration["Keycloack:AudienceKawaClient"],
                            Configuration["Keycloack:AudienceRealmManagement"]

                        }
                };
                //options.Events = new JwtBearerEvents()
                //{
                //    OnTokenValidated = c =>
                //    {
                //        Console.WriteLine("User successfully authenticated");
                //        return Task.CompletedTask;
                //    },
                //    OnAuthenticationFailed = c =>
                //    {
                //        c.NoResult();
                //        c.Response.StatusCode = 500;
                //        c.Response.ContentType = "text/plain";

                //        return c.Response.WriteAsync("An error occured processing your authentication.");
                //    }
                //};
            });

            // ** This is to configure Swagger to generate the API documentation **//
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


                // Add security definitions
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
                .AddInfrastructureServices(Configuration)
                .AddPresistenceServices(Configuration)
                .AddApplicationServices(Configuration)
                 .AddServicesNeededForController(Configuration);

            var app = builder.Build();

            // This will capture HTTP metrics
            app.UseHttpMetrics();
            
            // Middleware for serving generated Swagger JSON document and Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI();

            // Middleware to redirect HTTP requests to HTTPS.
            app.UseHttpsRedirection();

            // Routing middleware to route requests
            app.UseRouting();

            // Middleware for authentication
            app.UseAuthentication();
            // Middleware for authorization
            app.UseAuthorization();

            // Routing to controllers
            app.UseEndpoints(endpoints =>
            {
                // Map the controllers within the UseEndpoints middleware
                endpoints.MapControllers();
                // This serves metrics via /metrics endpoint
                endpoints.MapMetrics();
            });

            await app.RunAsync();
        }
    }

    public static class DependencyInjectionsHelpers
    {
        public static void AddServicesNeededForController(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
        }

    }

    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;

            // flatten resource_access because Microsoft identity model doesn't support nested claims
            // by map it to Microsoft identity model, because automatic JWT bearer token mapping already processed here
            if (claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim((claim) => claim.Type == "realm_access"))
            {
                var userRole = claimsIdentity.FindFirst((claim) => claim.Type == "realm_access");

                var content = Newtonsoft.Json.Linq.JObject.Parse(userRole.Value);

                foreach (var role in content["roles"])
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                }
            }

            return Task.FromResult(principal);
        }
    }

}
