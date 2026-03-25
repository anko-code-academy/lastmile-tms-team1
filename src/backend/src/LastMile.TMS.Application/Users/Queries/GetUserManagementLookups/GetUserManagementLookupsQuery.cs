using LastMile.TMS.Domain.Common;
using LastMile.TMS.Application.Users.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Users.Queries.GetUserManagementLookups;

public record GetUserManagementLookupsQuery : IRequest<Result<UserManagementLookupsDto>>;
