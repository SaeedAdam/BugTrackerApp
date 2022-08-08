using BugTracker.Data;
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
    private readonly ApplicationDbContext _context;
    private readonly IBTRolesService _rolesService;
    private readonly IBTLookupService _lookupsService;
    private readonly IBTFileService _fileService;
    private readonly IBTProjectService _projectService;
    private readonly UserManager<BTUser> _userManager;
    private readonly IBTCompanyInfoService _companyInfoService;

    public ProjectsController(ApplicationDbContext context, IBTRolesService rolesService, IBTLookupService lookupsService, IBTFileService fileService, IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService)
    {
        _context = context;
        _rolesService = rolesService;
        _lookupsService = lookupsService;
        _fileService = fileService;
        _projectService = projectService;
        _userManager = userManager;
        _companyInfoService = companyInfoService;
    }

    // GET: Projects
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
        return View(await applicationDbContext.ToListAsync());
    }

    public async Task<IActionResult> MyProjects()
    {
        string userId = _userManager.GetUserId(User);

        var projects = await _projectService.GetUserProjectsAsync(userId);

        return View(projects);
    }

    public async Task<IActionResult> AllProjects()
    {
        int companyId = User.Identity.GetCompanyId().Value;

        List<Project> projects = new();

        if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
        {
            projects = await _companyInfoService.GetAllProjectsAsync(companyId);
        }
        else
        {
            projects = await _projectService.GetAllProjectsByCompany(companyId);
        }

        return View(projects);
    }

    public async Task<IActionResult> ArchivedProjects()
    {
        int companyId = User.Identity.GetCompanyId().Value;

        var projects = await _projectService.GetArchivedProjectsByCompany(companyId);

        return View(projects);
    }

    // GET: Projects/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        int companyId = User.Identity.GetCompanyId().Value;


        var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    // GET: Projects/Create
    public async Task<IActionResult> Create()
    {
        int companyId = User.Identity.GetCompanyId().Value;

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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
    {
        if (model is not null)
        {
            int companyId = User.Identity.GetCompanyId().Value;

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
                {
                    await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //TODO: Redirect to All Projects
            return RedirectToAction("Index");
        }

        return RedirectToAction("Create");
    }

    // GET: Projects/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        int companyId = User.Identity.GetCompanyId().Value;

        AddProjectWithPMViewModel model = new();

        model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

        // Load SelectList with data ie. PMList & PriorityList 
        model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId),
            "Id", "FullName");
        model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");

        return View(model);
    }

    // POST: Projects/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
    {
        if (model is not null)
        {
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
                {
                    await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                }

                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return RedirectToAction("Edit");
    }

    // GET: Projects/Archive/5
    public async Task<IActionResult> Archive(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        int companyId = User.Identity.GetCompanyId().Value;

        Project project = null;

        if (User.IsInRole(Roles.Admin.ToString()))
        {
            project = await _projectService.GetProjectByIdForAdminAsync(id.Value);
        }
        else
        {
            project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
        }

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    // POST: Projects/Archive/5
    [HttpPost, ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveConfirmed(int id)
    {
        int companyId = User.Identity.GetCompanyId().Value;

        var project = await _projectService.GetProjectByIdAsync(id, companyId);

        await _projectService.ArchiveProjectAsync(project);

        return RedirectToAction(nameof(Index));
    }


    // GET: Projects/Restore/5
    public async Task<IActionResult> Restore(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        int companyId = User.Identity.GetCompanyId().Value;

        Project project = null;

        if (User.IsInRole(Roles.Admin.ToString()))
        {
            project = await _projectService.GetProjectByIdForAdminAsync(id.Value);
        }
        else
        {
            project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
        }

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    // POST: Projects/Restore/5
    [HttpPost, ActionName("Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreConfirmed(int id)
    {
        int companyId = User.Identity.GetCompanyId().Value;

        var project = await _projectService.GetProjectByIdAsync(id, companyId);

        await _projectService.RestoreProjectAsync(project);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult AssignUsers()
    {
        throw new NotImplementedException();
    }
}