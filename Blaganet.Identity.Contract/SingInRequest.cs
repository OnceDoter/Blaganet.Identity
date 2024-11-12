namespace Blaganet.Identity.Contract;

public sealed record class SingInRequest(
    string Email, 
    string Password);