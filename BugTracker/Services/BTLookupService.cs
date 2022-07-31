using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services;

public class BTLookupService : IBTLookupService
{
    private readonly ApplicationDbContext _context;

    public BTLookupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<TicketStatus>> GetTicketStatusesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<TicketType>> GetTicketTypesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
    {
        try
        {
            return await _context.ProjectPriorities.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
