﻿using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Controllers;

public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Notifications
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Notifications.Include(n => n.Recipient).Include(n => n.Sender).Include(n => n.Ticket);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: Notifications/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Notifications == null)
        {
            return NotFound();
        }

        var notification = await _context.Notifications
            .Include(n => n.Recipient)
            .Include(n => n.Sender)
            .Include(n => n.Ticket)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (notification == null)
        {
            return NotFound();
        }

        return View(notification);
    }

    // GET: Notifications/Create
    public IActionResult Create()
    {
        ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "FirstName");
        ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FirstName");
        ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
        return View();
    }

    // POST: Notifications/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,TicketId,Title,Message,Created,RecipientId,SenderId,Viewed")] Notification notification)
    {
        if (ModelState.IsValid)
        {
            _context.Add(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "FirstName", notification.RecipientId);
        ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FirstName", notification.SenderId);
        ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
        return View(notification);
    }

    // GET: Notifications/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Notifications == null)
        {
            return NotFound();
        }

        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
        {
            return NotFound();
        }
        ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "FirstName", notification.RecipientId);
        ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FirstName", notification.SenderId);
        ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
        return View(notification);
    }

    // POST: Notifications/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Title,Message,Created,RecipientId,SenderId,Viewed")] Notification notification)
    {
        if (id != notification.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(notification);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(notification.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "FirstName", notification.RecipientId);
        ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FirstName", notification.SenderId);
        ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", notification.TicketId);
        return View(notification);
    }

    // GET: Notifications/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Notifications == null)
        {
            return NotFound();
        }

        var notification = await _context.Notifications
            .Include(n => n.Recipient)
            .Include(n => n.Sender)
            .Include(n => n.Ticket)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (notification == null)
        {
            return NotFound();
        }

        return View(notification);
    }

    // POST: Notifications/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Notifications == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Notifications'  is null.");
        }
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NotificationExists(int id)
    {
        return (_context.Notifications?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}