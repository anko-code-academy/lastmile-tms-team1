using MediatR;

namespace LastMile.TMS.Application.Features.Auth.Commands.Login;

public record LoginCommand(string UserName, string Password) : IRequest<TokenResponse>;