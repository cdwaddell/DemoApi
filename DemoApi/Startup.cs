using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using DemoApi.Controllers;
using DemoApi.Data;
using DemoApi.Data.Entities;
using DemoApi.Models.Core;
using DemoApi.MVC;
using DemoApi.Swagger;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DemoApi
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static IHostingEnvironment CurrentEnvironment { get; set; }
        public static bool Debugging { get; set; }
        
        public Startup(IHostingEnvironment env)
        {
            CurrentEnvironment = env;

#if DEBUG
            Debugging = true;
#else
            Debugging = false;
#endif

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables("DemoApi_");
            
            Configuration = builder.Build();
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddSingleton(CurrentEnvironment);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped(x => x.GetService<IHttpContextAccessor>()?.HttpContext?.User);

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var connectionString = Configuration.GetConnectionString("DemoApi");

            services.AddDbContext<DemoAppDbContext>(builder =>
                builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly))
            );

            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.WithExposedHeaders("WWW-Authenticate");
                });
            });

            services.AddMvc(options =>
            {
                options.UseCentralRoutePrefix(new RouteAttribute("api/v{version:apiVersion}"));
                if (!Debugging)
                {
                    options.Filters.Add(typeof(RequireHttpsAttribute));
                }
                options.Filters.Add(typeof(ValidationActionFilterAttribute));
                options.Filters.Add(new ProducesAttribute("application/json"));

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireScope(ApiConstants.PublicScope)
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddApiVersioning();

            var apiName = Configuration["ApiName"];
            var apiSecret = Configuration["ApiSecret"];
            var authority = Configuration["Authority"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddIdentityServerAuthentication(options =>
            {
                // base-address of your identityserver
                options.Authority = authority;
                options.RequireHttpsMetadata = !Debugging;

                options.TokenRetriever = TokenRetrieval.FromAuthHeaderWithFallback();

                // name of the API resource
                options.ApiName = apiName;
                options.ApiSecret = apiSecret;

                options.JwtBearerEvents.OnTokenValidated += async ctx =>
                {
                    var sub = ctx.Principal.FindFirst(JwtClaimTypes.Subject).Value;
                    
                    var dbContext = ctx.HttpContext.RequestServices.GetRequiredService<DemoAppDbContext>();
                    var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(x => x.Sub == sub);
                    
                    if (profile == null)
                    {
                        var nickname = ctx.Principal.FindFirst(JwtClaimTypes.NickName)?.Value ?? "Anonymous";
                        profile = new UserProfile {Sub = sub, DisplayName = nickname};
                        var entity = await dbContext.UserProfiles.AddAsync(profile);
                        try
                        {
                            await dbContext.SaveChangesAsync();
                        }
                        catch
                        {
                            //multiple requests must have come in, lets kill this one as the other one was first
                            entity.State = EntityState.Unchanged;
                        }
                    }
                };

                options.EnableCaching = true;
                options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default
            });

            services.AddSwaggerGen(options =>
            {
                //options.MapType<Stream>(() => new Schema { Type = "file" });
                options.DocInclusionPredicate((version, apiDescription) =>
                {
                    var prepend = apiDescription.RelativePath.StartsWith("/");

                    if (apiDescription.ControllerAttributes()
                            .OfType<ApiVersionAttribute>()
                            .SelectMany(attr => attr.Versions)
                            .All(v => $"v{v.MajorVersion}.{v.MinorVersion}{v.Status}" != version)
                        && !apiDescription.ControllerAttributes()
                            .OfType<ApiVersionNeutralAttribute>().Any()
                    )
                        return false;

                    var values = apiDescription.RelativePath
                        .Split('/')
                        .Select(v => v.Replace("v{version}", version));

                    apiDescription.RelativePath = prepend ? "/" : "" + string.Join("/", values);

                    var versionParameter = apiDescription.ParameterDescriptions
                        .SingleOrDefault(p => p.Name == "version");

                    if (versionParameter != null)
                        apiDescription.ParameterDescriptions.Remove(versionParameter);

                    return true;
                });

                foreach (var version in ApiConstants.Versions)
                {
                    options.SwaggerDoc("v" + version, new Info
                    {
                        Version = version,
                        Title = $"DemoApi {version}",
                        Description = $"Version {version} of the DemoApi application."
                    });
                }

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                options.AddSecurityDefinition("oidc", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = Configuration["Authority"] + "/connect/authorize",
                    Scopes = ApiConstants.Scopes
                });

                // Assign scope requirements to operations based on AuthorizeAttribute
                options.OperationFilter<SecurityRequirementsOperationFilter>();

                //// Documentation for returning files
                //options.OperationFilter<FileFilter>();

                //// Documentation for uploading files
                //options.OperationFilter<FileUploadFilter>();

                //Setup comments based on XML data
                var xmlDocPath = Path.Combine(AppContext.BaseDirectory, "DemoApi.xml");
                options.IncludeXmlComments(xmlDocPath);
                options.DescribeAllEnumsAsStrings();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime, IHostingEnvironment env, IServiceScopeFactory factory, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAll");

            if (CurrentEnvironment.IsDevelopment() || Debugging)
                loggerFactory = loggerFactory.AddDebug();
            else
                loggerFactory = loggerFactory.WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", LogLevel.Warning },
                    { "System", LogLevel.Warning },
                    { "DemoApi", LogLevel.Debug }
                });

            if(Debugging)
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                using (var serviceScope = factory.CreateScope())
                {
                    var provider = serviceScope.ServiceProvider;
                    var settings = provider.GetService<IOptions<MvcJsonOptions>>().Value.SerializerSettings;
                    SelfLinkViewModel.ActionContextAccessor = provider.GetService<IActionContextAccessor>();

                    JsonConvert.DefaultSettings = () => settings;

                    if (CurrentEnvironment.IsDevelopment())
                    {
                        var context = provider.GetService<DemoAppDbContext>();

                        if (context.Database.GetPendingMigrations().Any())
                        {
                            context.Database.Migrate();
                        }
                    }
                }
            });
            
            var apiPath = new PathString("/api");
            
            //non-API calls get added MVC functionality
            app.UseWhen(context => !context.Request.Path.StartsWithSegments(apiPath), builder =>
            {
                if (CurrentEnvironment.IsDevelopment() || Debugging)
                {
                    builder.UseDeveloperExceptionPage();
                }
                else
                {
                    builder.UseExceptionHandler("/Error");
                }

                builder.UseStaticFiles();
                builder.UseSwagger();

                app.UseSwaggerUI(options =>
                {
                    options.InjectStylesheet("../../css/theme-outline.css");
                    options.InjectStylesheet("../../css/site.css");
                    options.RoutePrefix = "swagger/ui";

                    foreach (var version in ApiConstants.Versions)
                    {
                        options.SwaggerEndpoint($"/swagger/v{version}/swagger.json", version);
                    }

                    options.OAuthClientId("Blog Swagger");
                    options.OAuthClientSecret("");
                    options.OAuthRealm("resource");
                    options.OAuthAppName("Swagger Client");
                    options.OAuthScopeSeparator(" ");
                    options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                });
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
