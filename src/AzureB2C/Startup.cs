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

namespace AzureB2C
{
    public class Startup
    {
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
            var configurationBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
            Configuration = configurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Tenant = "******.onmicrosoft.com";
            ClientId = "";
            AppKey = "";
            // These are the names I used in the portal for the policies
            SignUpPolicyId = "B2C_1_Sign_Up";
            SignInPolicyId = "B2C_1_Sign_In";
            ProfilePolicyId = "B2C_1_Edit";
            // Set to localhost at port 44301: Change to whereever you need AAD B2C to send them after auth, but url encode the string
            RedirectUrl = "https%3a%2f%2flocalhost%3a5000%2f";
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseCookieAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                }
            );
            app.UseOpenIdConnectAuthentication(options =>
                {
                    options.ClientId = ClientId;
                    options.Authority = $"https://login.microsoftonline.com/{Tenant}/v2.0/.well-known/openid-configuration";
                    options.ResponseType = "id_token";
                    options.ConfigurationManager = new PolicyConfigurationManager(options.Authority, new string[] { SignUpPolicyId, SignInPolicyId, ProfilePolicyId });
                    options.AutomaticAuthenticate = true;
                    options.SignInScheme = "Cookies";

                    // ***********************************************************************************
                    // ****** TODO: Remove this when nonce can be produced in a normal ChallengeResponse *
                    options.ProtocolValidator.RequireNonce = false;
                    // ***********************************************************************************
                    //options.Notification = new OpenIdConnectNotifications {
                    //    RedirectToIdentityProvider = (context) =>
                    //    {
                    //        if (context.HttpContext.User.Identity.IsAuthenticated)
                    //        {
                    //            if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                    //            {
                    //                context.HandleResponse();
                    //                context.HttpContext.Response.Redirect($"https://login.microsoftonline.com/{Tenant}/oauth2/v2.0/logout?p={SignInPolicyId}&redirect_uri={RedirectUrl}");
                    //            }
                    //            else
                    //            {
                    //                context.HandleResponse();
                    //                context.HttpContext.Response.Redirect(Uri.EscapeUriString("/Home/Error?message=You are not authorized to access the requested resource."));
                    //            }
                    //        }
                    //        return Task.FromResult(0);
                    //    },
                    //    AuthenticationFailed = (context) => {
                    //        context.HandleResponse();
                    //        if (context.ProtocolMessage.Error != null)
                    //        {
                    //            context.Response.Redirect(Uri.EscapeUriString("/Home/Error?message=Error: " + System.Uri.EscapeUriString(context.ProtocolMessage.Error.ToString())));
                    //        }
                    //        else
                    //        {
                    //            context.Response.Redirect(Uri.EscapeUriString("/Home/Error?message=Error: An unknown error occurred and the authentication failed. Please contact the IT Department."));
                    //        }
                    //        return Task.FromResult(0);
                    //    },
                    //};
                }
            );
            app.UseMvcWithDefaultRoute();
        }
    }
}