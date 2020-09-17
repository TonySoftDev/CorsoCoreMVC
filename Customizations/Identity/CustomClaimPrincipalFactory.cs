using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyCourse.Customizations.Identity
{
    public class CustomClaimPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public CustomClaimPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("FullName", user.FullName));
            return identity;
        }
    }
}
