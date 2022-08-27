using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services;

public class BTRolesService : IBTRolesService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<BTUser> _userManager;

    public BTRolesService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager,
        UserManager<BTUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
    {
        var result = await _userManager.IsInRoleAsync(user, roleName);

        return result;
    }

    public async Task<List<IdentityRole>> GetRolesAsync()
    {
        try
        {
            return await _context.Roles.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
    {
        IEnumerable<string> result = await _userManager.GetRolesAsync(user);

        return result;
    }

    public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
    {
        var result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;

        return result;
    }

    public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
    {
        var result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;

        return result;
    }

    public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
    {
        var result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;

        return result;
    }

    public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
    {
        var users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();

        var result = users.Where(u => u.CompanyId == companyId).ToList();

        return result;
    }

    public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
    {
        var userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();

        var roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();

        var result = roleUsers.Where(u => u.CompanyId == companyId).ToList();

        return result;
    }

    public async Task<string> GetRoleNameByIdAsync(string roleId)
    {
        var role = _context.Roles.Find(roleId);

        var result = await _roleManager.GetRoleNameAsync(role);

        return result;
    }
}