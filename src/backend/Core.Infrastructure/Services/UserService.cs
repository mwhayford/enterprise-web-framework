using Microsoft.EntityFrameworkCore;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;

namespace Core.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User> RegisterUserAsync(
        string firstName,
        string lastName,
        Email email,
        string? googleId = null,
        string? profilePictureUrl = null)
    {
        var user = new User(firstName, lastName, email, googleId, profilePictureUrl);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(Email email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByGoogleIdAsync(string googleId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<User> UpdateUserProfileAsync(Guid userId, string firstName, string lastName, string? profilePictureUrl = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        user.UpdateProfile(firstName, lastName, profilePictureUrl);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.Deactivate();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.Activate();
        await _context.SaveChangesAsync();
        return true;
    }
}
