using BugTracker.Models;

namespace BugTracker.Services.Interfaces;

public interface IBTLookupService
{
    Task<List<TicketPriority>> GetTicketPrioritiesAsync();
    Task<List<TicketStatus>> GetTicketStatusesAsync();
    Task<List<TicketType>> GetTicketTypesAsync();
    Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
}