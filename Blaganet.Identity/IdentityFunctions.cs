using System.Security.Claims;
using Blaganet.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Blaganet.Identity;

// ReSharper disable once ClassNeverInstantiated.Global
internal partial class IdentityFunctions(
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    SignInManager<IdentityUser> signInManager,
    AuthenticationContext context)
{
    public sealed class UserClaimsPrincipalFactory(UserManager<IdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor) 
        : UserClaimsPrincipalFactory<IdentityUser>(userManager, optionsAccessor)
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            var principal = await base.CreateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            
            principal.Identities.First().AddClaims([
                new Claim(IdentityDefaults.Claims.UserEmail, user.Email ?? string.Empty),
                new Claim(IdentityDefaults.Claims.UserName, user.UserName ?? string.Empty),
                new Claim(IdentityDefaults.Claims.UserPhone, user.PhoneNumber ?? string.Empty),
                new Claim(IdentityDefaults.Claims.UserPhoneConfirmed, user.PhoneNumberConfirmed.ToString()),
                new Claim(IdentityDefaults.Claims.UserRoles, string.Join(IdentityDefaults.Claims.Delemiter, roles)),
            ]);
            
            return principal;
        }
    }
}