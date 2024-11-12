namespace Blaganet.Identity.Contract;

public sealed record class SingUpRequest(
    string Email, 
    string Password);