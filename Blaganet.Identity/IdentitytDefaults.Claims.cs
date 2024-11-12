namespace Blaganet.Identity;

internal partial class IdentityDefaults
{
    public sealed record Claims(string Name)
    {
        public const string Delemiter = ",";
        
        public static readonly Claims UserName = new("user-name");
        public static readonly Claims UserEmail = new("user-email");
        public static readonly Claims UserPhone = new("user-phone");
        public static readonly Claims UserPhoneConfirmed = new("user-phone-confirmed");
        public static readonly Claims UserRoles = new("user-roles");

        public static implicit operator string(Claims roles) => roles.ToString();
        public override string ToString()
        {
            return this.Name;
        }
    }
}