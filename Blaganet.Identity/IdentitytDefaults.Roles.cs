using Microsoft.AspNetCore.Identity;

namespace Blaganet.Identity;

internal partial class IdentityDefaults
{
    public sealed record Roles(string Name)
    {
        public static readonly Roles SystemAdministrator = new("system-administrator");

        public override string ToString() => this.Name;
        
        public static implicit operator string(Roles roles) => roles.ToString();
        
        public static explicit operator IdentityRole(Roles roles) => new(roles);
    }
}