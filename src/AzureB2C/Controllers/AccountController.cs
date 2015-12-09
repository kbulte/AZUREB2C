using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication.Cookies;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;

namespace AzureB2C.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public void LogIn(OpenIdConnectOptions openIdConnectOptions)
        {
            //return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
            {
                // Generate the nonce and give the user a claim for it
                // BROKEN: Can't get StringDataFormat.Protect()
                //string nonce = openIdConnectOptions.ProtocolValidator.GenerateNonce();
                //Context.User.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim("nonce", nonce) }, OpenIdConnectAuthenticationDefaults.AuthenticationScheme));
                //string protectedNonce = openIdConnectOptions.StringDataFormat.Protect(nonce);
                //Response.Cookies.Append(".AspNet.OpenIdConnect.Nonce." + protectedNonce, "N", new CookieOptions { HttpOnly = true, Secure = Request.IsHttps });

                HttpContext.Response.Redirect(
                    $"https://login.microsoftonline.com/{Startup.Tenant}/oauth2/v2.0/authorize" +
                    $"?p={Startup.SignInPolicyId}" +
                    $"&client_id={Startup.ClientId}" +
                    "&nonce = defaultNonce" +
                    $"&redirect_uri={Startup.RedirectUrl}" +
                    "&scope=openid" +
                    "&response_type=id_token" +
                    "&prompt=login", false
                );
            }
            else
            {
                HttpContext.Response.Redirect("/");
            }
        }

        // GET: /Account/LogOut
        [HttpGet]
        public async Task LogOut()
        {
            await HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // GET: /Account/Edit
        [HttpGet]
        public void Edit()
        {
            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.Response.Redirect(
                    $"https://login.microsoftonline.com/{Startup.Tenant}/oauth2/v2.0/authorize" +
                    $"?p={Startup.ProfilePolicyId}" +
                    $"&client_id={Startup.ClientId}" +
                    "&nonce = defaultNonce" +
                    $"&redirect_uri={Startup.RedirectUrl}" +
                    "&scope=openid" +
                    "&response_type=id_token" +
                    "&prompt=login"
                );
            }
            else
            {
                HttpContext.Response.Redirect("/");
            }
        }

        // GET: /Account/Signup
        [HttpGet]
        public void Signup(OpenIdConnectOptions openIdConnectOptions)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.Response.Redirect(
                    $"https://login.microsoftonline.com/{Startup.Tenant}/oauth2/v2.0/authorize" +
                    $"?p={Startup.SignUpPolicyId}" +
                    $"&client_id={Startup.ClientId}" +
                    "&nonce = defaultNonce" +
                    $"&redirect_uri={Startup.RedirectUrl}" +
                    "&scope=openid" +
                    "&response_type=id_token" +
                    "&prompt=login", false
                );
            }
            else
            {
                HttpContext.Response.Redirect("/");
            }
        }
    }
}
