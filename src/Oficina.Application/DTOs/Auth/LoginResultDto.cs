namespace Oficina.Application.DTOs.Auth;

public class LoginResultDto
{
    public required string Token { get; set; }
    public DateTime ExpiraEm { get; set; }
}
