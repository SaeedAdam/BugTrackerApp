using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels;

public class ProjectsMemberViewModel
{
    public Project Project { get; set; }
    public MultiSelectList Users { get; set; }
    public List<string> SelectedUsers { get; set; }
}
