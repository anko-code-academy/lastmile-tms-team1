using MediatR;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<Result<List<UserDto>>>;
