using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;

namespace Ludix.Controllers
{
    [Authorize]
    public class MyUsersController : Controller
    {
        private readonly LudixContext _context;

        public MyUsersController(LudixContext context)
        {
            _context = context;
        }

        // GET: MyUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.MyUser.ToListAsync());
        }

        // GET: MyUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myUser = await _context.MyUser
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (myUser == null)
            {
                return NotFound();
            }

            return View(myUser);
        }

        // GET: MyUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,Email,Balance,CreatedAt,AspUser")] MyUser myUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(myUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myUser);
        }

        // GET: MyUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myUser = await _context.MyUser.FindAsync(id);
            if (myUser == null)
            {
                return NotFound();
            }
            return View(myUser);
        }

        // POST: MyUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Email,Balance,CreatedAt,AspUser")] MyUser myUser)
        {
            if (id != myUser.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(myUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MyUserExists(myUser.UserId))
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
            return View(myUser);
        }

        // GET: MyUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myUser = await _context.MyUser
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (myUser == null)
            {
                return NotFound();
            }

            return View(myUser);
        }

        // POST: MyUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myUser = await _context.MyUser.FindAsync(id);
            if (myUser != null)
            {
                _context.MyUser.Remove(myUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyUserExists(int id)
        {
            return _context.MyUser.Any(e => e.UserId == id);
        }
    }
}
