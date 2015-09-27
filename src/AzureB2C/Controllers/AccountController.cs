using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication.Cookies;
using System.Threading.Tasks;

namespace AzureB2C.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public void LogIn(OpenIdConnectAuthenticationOptions openIdConnectOptions)
        {
            if (Context.User == null || !Context.User.Identity.IsAuthenticated)
            {
                // Generate the nonce and give the user a claim for it
                // BROKEN: Can't get StringDataFormat.Protect()
                //string nonce = openIdConnectOptions.ProtocolValidator.GenerateNonce();
                //Context.User.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim("nonce", nonce) }, OpenIdConnectAuthenticationDefaults.AuthenticationScheme));
                //string protectedNonce = openIdConnectOptions.StringDataFormat.Protect(nonce);
                //Response.Cookies.Append(".AspNet.OpenIdConnect.Nonce." + protectedNonce, "N", new CookieOptions { HttpOnly = true, Secure = Request.IsHttps });

                Context.Response.Redirect(
                    $"https://login.microsoftonline.com/{Startup.Tenant}/oauth2/v2.0/authorize" +
                    $"?p={Startup.SignInPolicyId}" +
                    $"&client_id={Startup.ClientId}" +
                    "&response_type=id_token" +
                    "&scope=openid" +
                    "&response_mode=form_post" +
                    $"&redirect_uri={Startup.RedirectUrl}"
                    // + "&nonce=" + nonce
                );
            }
            else
            {
                Context.Response.Redirect("/");
            }
        }

        // GET: /Account/LogOut
        [HttpGet]
        public async Task LogOut()
        {
            await Context.Authentication.SignOutAsync(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
            await Context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // GET: /Account/Edit
        [HttpGet]
        public void Edit()
        {
            if (Context.User != null && Context.User.Identity.IsAuthenticated)
            {
                Context.Response.Redirect(
                    $"https://login.microsoftonline.com/{Startup.Tenant}/oauth2/v2.0/authorize" +
                    $"?p={Startup.ProfilePolicyId}" +
                    $"&client_id={Startup.ClientId}" +
                    "&response_type=id_token" +
                    "&scope=openid" +
                    "&response_mode=form_post" +
                    $"&redirect_uri={Startup.RedirectUrl}"
                );
            }
            else
            {
                Context.Response.Redirect("/");
            }
        }

        // GET: /Account/Signup
        [HttpGet]
        public void Signup(OpenIdConnectAuthenticationOptions openIdConnectOptions)
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                Context.Response.Redirect(
                    $"https://login.microsoftonline.com/{Startup.Tenant}/oauth2/v2.0/authorize" +
                    $"&client_id={Startup.SignUpPolicyId}" +
                    $"&client_id={Startup.ClientId}" +
                    "&response_type=id_token" +
                    "&scope=openid" +
                    "&response_mode=form_post" +
                    $"&redirect_uri={Startup.RedirectUrl}"
                );
            }
            else
            {
                Context.Response.Redirect("/");
            }
        }
    }
}
