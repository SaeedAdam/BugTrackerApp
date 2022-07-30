using System.Security.Claims;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BugTracker.Services.Factories;

public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
{
    public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        IOptions<IdentityOptions> optionsAccessor)
    : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
        identity.AddClaim(new Claim("FullName", user.FullName));
        return identity;
    }
}
