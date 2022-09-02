using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IBTCompanyInfoService _companyInfoService;
    private readonly IBTFileService _fileService;
    private readonly IBTLookupService _lookupsService;
    private readonly IBTProjectService _projectService;
    private readonly IBTRolesService _rolesService;
    private readonly UserManager<BTUser> _userManager;

    public ProjectsController(IBTRolesService rolesService, IBTLookupService lookupsService, IBTFileService fileService,
        IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService)
    {
        _rolesService = rolesService;
        _lookupsService = lookupsService;
        _fileService = fileService;
        _projectService = projectService;
        _userManager = userManager;
        _companyInfoService = companyInfoService;
    }

    public async Task<IActionResult> MyProjects()
    {
        var userId = _userManager.GetUserId(User);

        List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

        return View(projects);
    }

    public async Task<IActionResult> AllProjects()
    {
        var companyId = User.Identity.GetCompanyId().Value;

        List<Project> projects = new();

        if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            projects = await _companyInfoService.GetAllProjectsAsync(companyId);
        else
            projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

        return View(projects);
    }

    public async Task<IActionResult> ArchivedProjects()
    {
        var companyId = User.Identity.GetCompanyId().Value;

        var projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);

        return View(projects);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> UnassignedProjects()
    {
        var companyId = User.Identity.GetCompanyId().Value;

        var projects = await _projectService.GetUnassignedProjectsAsync(companyId);

        return View(projects);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpGet]
    public async Task<IActionResult> AssignPM(int id)
    {
        var companyId = User.Identity.GetCompanyId().Value;

        AssignPMViewModel model = new();

        model.Project = await _projectService.GetProjectByIdAsync(id, companyId);
        model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId),
            "Id", "FullName");

        return View(model);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignPM(AssignPMViewModel model)
    {
        if (!string.IsNullOrEmpty(model.PMID))
        {
            await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);

            return RedirectToAction(nameof(Details), new { id = model.Project.Id });
        }

        return RedirectToAction(nameof(AssignPM), new { projectId = model.Project.Id });
    }

    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    [HttpGet]
    public async Task<IActionResult> AssignMembers(int id)
    {
        ProjectsMemberViewModel model = new();

        var companyId = User.Identity.GetCompanyId().Value;

        model.Project = await _projectService.GetProjectByIdAsync(id, companyId);

        var developers = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId);
        var submitters = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId);

        var companyMembers = developers.Concat(submitters).ToList();

        var projectsMembers = model.Project.Members.Select(m => m.Id).ToList();


        model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectsMembers);

        return View(model);
    }

    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMembers(ProjectsMemberViewModel model)
    {
        if (model.SelectedUsers is not null)
        {
            var membersId = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                .Select(m => m.Id).ToList();


            // Remove current members
            foreach (var member in membersId)
                await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);

            // Add new members
            foreach (var member in model.SelectedUsers)
                await _projectService.AddUserToProjectAsync(member, model.Project.Id);

            return RedirectToAction(nameof(Details), new { id = model.Project.Id });
        }

        return RedirectToAction(nameof(AssignMembers), new { projectId = model.Project.Id });
    }


    // GET: Projects/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var companyId = User.Identity.GetCompanyId().Value;


        var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

        if (project == null) return NotFound();

        return View(project);
    }

    // GET: Projects/Create
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    public async Task<IActionResult> Create()
    {
        var companyId = User.Identity.GetCompanyId().Value;

        AddProjectWithPMViewModel model = new();

        // Load SelectList with data ie. PMList & PriorityList 
        model.PMList =
            new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId),
                "Id", "FullName");

        model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");

        return View(model);
    }

    // POST: Projects/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
    {
        if (model is not null)
        {
            var companyId = User.Identity.GetCompanyId().Value;

            try
            {
                if (model.Project.ImageFormFile is not null)
                {
                    model.Project.ImageFileData =
                        await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                }

                model.Project.CompanyId = companyId;

                await _projectService.AddNewProjectAsync(model.Project);

                // Add PM if one was chosen
                if (!string.IsNullOrEmpty(model.PMId))
                    await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //TODO: Redirect to All Projects
            return RedirectToAction("MyProjects");
        }

        return RedirectToAction("Create");
    }

    // GET: Projects/Edit/5
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    public async Task<IActionResult> Edit(int? id)
    {
        var companyId = User.Identity.GetCompanyId().Value;

        AddProjectWithPMViewModel model = new();

        model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

        // Load SelectList with data ie. PMList & PriorityList 
        model.PMList = new SelectList(
            await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId),
            "Id", "FullName");
        model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");

        return View(model);
    }

    // POST: Projects/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
    {
        if (model is not null)
            try
            {
                if (model.Project.ImageFormFile is not null)
                {
                    model.Project.ImageFileData =
                        await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                }


                await _projectService.UpdateProjectAsync(model.Project);

                // Add PM if one was chosen
                if (!string.IsNullOrEmpty(model.PMId))
                    await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);

                return RedirectToAction("MyProjects");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProjectExists(model.Project.Id)) return NotFound();

                Console.WriteLine("Error Editing project.");
                throw;
            }

        return RedirectToAction("Edit");
    }

    // GET: Projects/Archive/5
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    public async Task<IActionResult> Archive(int? id)
    {
        if (id == null) return NotFound();

        var companyId = User.Identity.GetCompanyId().Value;

        Project project = null;

        if (User.IsInRole(Roles.Admin.ToString()))
            project = await _projectService.GetProjectByIdForAdminAsync(id.Value);
        else
            project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

        if (project == null) return NotFound();

        return View(project);
    }

    // POST: Projects/Archive/5
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    [HttpPost]
    [ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveConfirmed(int id)
    {
        var companyId = User.Identity.GetCompanyId().Value;

        var project = await _projectService.GetProjectByIdAsync(id, companyId);

        await _projectService.ArchiveProjectAsync(project);

        return RedirectToAction(nameof(MyProjects));
    }


    // GET: Projects/Restore/5
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    public async Task<IActionResult> Restore(int? id)
    {
        if (id == null) return NotFound();

        var companyId = User.Identity.GetCompanyId().Value;

        Project project = null;

        if (User.IsInRole(Roles.Admin.ToString()))
            project = await _projectService.GetProjectByIdForAdminAsync(id.Value);
        else
            project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

        if (project == null) return NotFound();

        return View(project);
    }

    // POST: Projects/Restore/5
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.ProjectManager)}")]
    [HttpPost]
    [ActionName("Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreConfirmed(int id)
    {
        var companyId = User.Identity.GetCompanyId().Value;

        var project = await _projectService.GetProjectByIdAsync(id, companyId);

        await _projectService.RestoreProjectAsync(project);

        return RedirectToAction(nameof(MyProjects));
    }

    private async Task<bool> ProjectExists(int id)
    {
        var companyId = User.Identity.GetCompanyId().Value;

        return (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Any(p => p.Id == id);
    }
}