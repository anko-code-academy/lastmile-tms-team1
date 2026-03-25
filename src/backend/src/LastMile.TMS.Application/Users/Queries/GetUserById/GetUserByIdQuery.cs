using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
