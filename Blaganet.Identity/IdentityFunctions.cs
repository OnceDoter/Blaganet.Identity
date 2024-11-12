using System.Security.Claims;
using Blaganet.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blaganet.Identity;

// ReSharper disable once ClassNeverInstantiated.Global
internal partial class IdentityFunctions(
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    IdentityFunctions.SignInManager signInManager,
    AuthorizationContext context)
{
    public class SignInManager(
        UserManager<IdentityUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<IdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<IdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<IdentityUser> confirmation)
        : SignInManager<IdentityUser>(
            userManager,
            contextAccessor,
            claimsFactory,
            optionsAccessor,
            logger,
            schemes,
            confirmation)
    {
        public override Task SignInWithClaimsAsync(
            IdentityUser user,
            AuthenticationProperties? authenticationProperties,
            IEnumerable<Claim> additionalClaims)
        {
            return Task.CompletedTask;
        }
    }
}