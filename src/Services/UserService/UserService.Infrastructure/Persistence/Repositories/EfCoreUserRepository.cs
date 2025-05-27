using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.UserService.Application.Interfaces.Persistence;
using SynopsisSI.Services.UserService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.UserService.Infrastructure.Persistence.Repositories;
public class EfCoreUserRepository : IUserRepository
{
    private readonly UserServiceDbContext _context;
    public EfCoreUserRepository(UserServiceDbContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));
    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(email) ? null : await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(username) ? null : await _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        await _context.Users.AddAsync(user, cancellationToken);
    }
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        _context.Entry(user).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}