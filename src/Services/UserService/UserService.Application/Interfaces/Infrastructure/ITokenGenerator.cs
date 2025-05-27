using SynopsisSI.Services.UserService.Domain.Entities;
using System;

namespace SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure;
public interface ITokenGenerator
{
    (string Token, DateTime Expiration) GenerateToken(User user);
}