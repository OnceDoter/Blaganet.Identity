namespace Blaganet.Identity.Contract;

public sealed record MeResponse(
    string UserName,
    string Email,
    string Phone,
    string PhoneConfirmed,
    string[] Roles);