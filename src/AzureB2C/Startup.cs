using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.DependencyInjection;
using AzureB2C.Policies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.Extensions.Logging;

namespace AzureB2C
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        public IConfiguration Configuration { get; set; }
        public static string Log = string.Empty;
        public static string ClientId = string.Empty;
        public static string Tenant = string.Empty;
        public static string AppKey = string.Empty;
        public static string PostLogoutRedirectUri = string.Empty;
        public static string RedirectUrl = string.Empty;
        public const string PolicyKey = "b2cpolicy";
        public static string SignUpPolicyId;
        public static string SignInPolicyId;
        public static string ProfilePolicyId;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var configurationBuilder = new ConfigurationBuilder();

            if (_env.IsDevelopment())
            {
                configurationBuilder.AddUserSecrets();
            }

            configurationBuilder.AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCaching();
            services.AddSession();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            Tenant = Configuration["Authentication:AzureAd:AADInstance"];
            ClientId = Configuration["Authentication:AzureAd:ClientId"];
            AppKey = Configuration["Authentication:AzureAd:AppKey"];
            // These are the names I used in the portal for the policies
            SignUpPolicyId = "B2C_1_Sign_Up";
            SignInPolicyId = "B2C_1_Sign_In";
            ProfilePolicyId = "B2C_1_Edit";
            // Set to localhost at port 44301: Change to whereever you need AAD B2C to send them after auth, but url encode the string
            RedirectUrl = Configuration["Authentication:AzureAd:RedirectUrl"];

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
                //app.UseApplicationInsightsExceptionTelemetry();
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.CookieName = "oldowanClaims";
                }
            );

            app.UseOpenIdConnectAuthentication(options =>
                {
                    options.ClientId = ClientId;
                    options.Authority = $"https://login.microsoftonline.com/{Tenant}/v2.0/.well-known/openid-configuration";
                    options.ResponseType = "id_token";
                    options.ConfigurationManager = new PolicyConfigurationManager(options.Authority, new string[] { SignUpPolicyId, SignInPolicyId, ProfilePolicyId });
                    options.AutomaticAuthenticate = true;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                }
            );

            app.UseMvcWithDefaultRoute();
        }
    }
}