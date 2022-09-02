namespace BugTracker.Models.ViewModels;

public class ProfileViewModel
{
    public Company Company { get; set; }
    public List<Project> Projects { get; set; }
    public List<Project> MyProjects { get; set; }
    public List<Ticket> Tickets { get; set; }
    public List<Ticket> MyTickets { get; set; }
    public List<BTUser> Members { get; set; }
}
