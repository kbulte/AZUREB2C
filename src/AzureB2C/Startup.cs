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
using System.Linq;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http;
using System.IdentityModel.Tokens;
using System.Threading;

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
            services.AddAuthentication(sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddCaching();
            services.AddSession();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug, true);
            loggerFactory.AddDebug(LogLevel.Debug);

            Tenant = Configuration["Authentication:AzureAd:AADInstance"];
            ClientId = Configuration["Authentication:AzureAd:ClientId"];
            AppKey = Configuration["Authentication:AzureAd:AppKey"];
            // These are the names I used in the portal for the policies
            SignUpPolicyId = "B2C_1_Sign_Up";
            SignInPolicyId = "B2C_1_Sign_In";
            ProfilePolicyId = "B2C_1_Edit";
            // Set to localhost at port 44301: Change to whereever you need AAD B2C to send them after auth, but url encode the string
            RedirectUrl = Configuration["Authentication:AzureAd:RedirectUrl"];
            //RedirectUrl = @"http%3A%2F%2Flocalhost%3A5000";

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                }
            );



            app.UseOpenIdConnectAuthentication(options =>
                {
                    options.ClientId = ClientId;
                    options.ClientSecret = AppKey;
                    options.Authority = $"https://login.microsoftonline.com/oldowanb2c.onmicrosoft.com/oauth2/v2.0/authorize";
                    options.ResponseType = OpenIdConnectResponseTypes.IdToken;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ConfigurationManager = new PolicyConfigurationManager("https://login.microsoftonline.com/oldowanb2c.onmicrosoft.com//.well-known/openid-configuration", new string[] { SignUpPolicyId, SignInPolicyId, ProfilePolicyId });
                    options.AutomaticAuthenticate = true;
                    options.CallbackPath = new PathString("/Home/Index");
                    options.ProtocolValidator.RequireNonce = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                    };
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToAuthenticationEndpoint = async (redirectContext) =>
                        {
                            if (redirectContext.HttpContext.User.Identity.IsAuthenticated)
                            {
                                //if (redirectContext.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                                //{
                                //    redirectContext.HandleResponse();
                                //    redirectContext.HttpContext.Response.Redirect($"https://login.microsoftonline.com/{Tenant}/oauth2/v2.0/logout?p={SignInPolicyId}&redirect_uri={RedirectUrl}");
                                //}
                                //else
                                //{
                                //    redirectContext.HandleResponse();
                                //    redirectContext.HttpContext.Response.Redirect(Uri.EscapeUriString("/Home/Error?message=You are not authorized to access the requested resource."));
                                //}

                                PolicyConfigurationManager mgr = redirectContext.Options.ConfigurationManager as PolicyConfigurationManager;
                                if (redirectContext.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                                {
                                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, SignUpPolicyId);
                                    redirectContext.ProtocolMessage.IssuerAddress = config.EndSessionEndpoint;
                                }
                                else
                                {
                                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, SignInPolicyId);
                                    redirectContext.ProtocolMessage.IssuerAddress = config.AuthorizationEndpoint;
                                }
                            }

                           // return Task.FromResult(0);
                        },

                        OnAuthenticationFailed = (context) =>
                        {
                            context.HandleResponse();
                            if (context.ProtocolMessage.Error != null)
                            {
                                context.Response.Redirect(Uri.EscapeUriString("/Home/Error?message=Error: " + System.Uri.EscapeUriString(context.ProtocolMessage.Error.ToString())));
                            }
                            else
                            {
                                context.Response.Redirect(Uri.EscapeUriString("/Home/Error?message=Error: An unknown error occurred and the authentication failed. Please contact the IT Department."));
                            }
                            return Task.FromResult(0);
                        }
                    };
                });


            app.UseMvcWithDefaultRoute();
        }
    }
}