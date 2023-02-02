using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IBTImageService _imageService;
    private readonly IBTProjectService _projectService;
    private readonly UserManager<BTUser> _userManager;
    private readonly IBTTicketService _ticketService;
    private readonly IBTCompanyInfoService _companyInfoService;

    public ProfileController(IBTImageService imageService, IBTProjectService projectService, UserManager<BTUser> userManager, IBTTicketService ticketService, IBTCompanyInfoService companyInfoService)
    {
        _imageService = imageService;
        _projectService = projectService;
        _userManager = userManager;
        _ticketService = ticketService;
        _companyInfoService = companyInfoService;
    }

    // GET: ProfileController/Details/5
    //[Route("u/{id:string}")]
    public async Task<ActionResult> UserDetails(string id)
    {
        if (id == null) return NotFound();

        if (_userManager.FindByIdAsync(id) is null)
        {
            return NotFound();
        }

        ProfileViewModel model = new();
        int companyId = User.Identity.GetCompanyId().Value;
        string userId = _userManager.GetUserId(User);

        model.MyProjects = await _projectService.GetUserProjectsAsync(userId);
        model.MyTickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);
        model.Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId);
        model.Projects = (await _companyInfoService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();
        model.Tickets = model.Projects.SelectMany(p => p.Tickets).Where(t => t.Archived == false).ToList();
        model.Members = model.Company.Members.ToList();

        return View(model);
    }

    //// GET: ProfileController/Create
    //public ActionResult Create()
    //{
    //    return View();
    //}

    //// POST: ProfileController/Create
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult Create(IFormCollection collection)
    //{
    //    try
    //    {
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch
    //    {
    //        return View();
    //    }
    //}

    //// GET: ProfileController/Edit/5
    //public ActionResult Edit(int id)
    //{
    //    return View();
    //}

    //// POST: ProfileController/Edit/5
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult Edit(int id, IFormCollection collection)
    //{
    //    try
    //    {
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch
    //    {
    //        return View();
    //    }
    //}

    //// GET: ProfileController/Delete/5
    //public ActionResult Delete(int id)
    //{
    //    return View();
    //}

    //// POST: ProfileController/Delete/5
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult Delete(int id, IFormCollection collection)
    //{
    //    try
    //    {
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch
    //    {
    //        return View();
    //    }
    //}
}
