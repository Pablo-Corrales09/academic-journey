using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Smart_Gym.Data;
using Smart_Gym.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smart_Gym.Controllers
{
    public class GimnasiosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GimnasiosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Gimnasios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Gimnasio.ToListAsync());
        }

        // GET: Gimnasios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gimnasio = await _context.Gimnasio
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gimnasio == null)
            {
                return NotFound();
            }

            return View(gimnasio);
        }

        // GET: Gimnasios/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Gimnasios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Direccion,Telefono")] Gimnasio gimnasio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gimnasio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gimnasio);
        }

        // GET: Gimnasios/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gimnasio = await _context.Gimnasio.FindAsync(id);
            if (gimnasio == null)
            {
                return NotFound();
            }
            return View(gimnasio);
        }

        // POST: Gimnasios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Direccion,Telefono")] Gimnasio gimnasio)
        {
            if (id != gimnasio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gimnasio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GimnasioExists(gimnasio.Id))
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
            return View(gimnasio);
        }

        // GET: Gimnasios/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gimnasio = await _context.Gimnasio
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gimnasio == null)
            {
                return NotFound();
            }

            return View(gimnasio);
        }

        // POST: Gimnasios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gimnasio = await _context.Gimnasio.FindAsync(id);
            if (gimnasio != null)
            {
                _context.Gimnasio.Remove(gimnasio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GimnasioExists(int id)
        {
            return _context.Gimnasio.Any(e => e.Id == id);
        }
    }
}
