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
    public class ClasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clases
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Clase.Include(c => c.Gimnasio);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Clases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clase = await _context.Clase
                .Include(c => c.Gimnasio)
                .FirstOrDefaultAsync(m => m.IdClase == id);
            if (clase == null)
            {
                return NotFound();
            }

            return View(clase);
        }

        // GET: Clases/Create
        [Authorize(Roles = "Administrador,Entrenador")]
        public IActionResult Create()
        {
            ViewData["IdGimnasio"] = new SelectList(_context.Gimnasio, "Id", "Nombre");
            return View();
        }

        // POST: Clases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Create([Bind("IdClase,Tipo,Nombre,FechaHora,IdGimnasio")] Clase clase)
        {
            try
            {
                _context.Add(clase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                throw;

            }
            ViewData["IdGimnasio"] = new SelectList(_context.Gimnasio, "Id", "Nombre", clase.IdGimnasio);
            return View(clase);
        }

        // GET: Clases/Edit/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clase = await _context.Clase.FindAsync(id);
            if (clase == null)
            {
                return NotFound();
            }
            ViewData["IdGimnasio"] = new SelectList(_context.Gimnasio, "Id", "Nombre", clase.IdGimnasio);
            return View(clase);
        }

        // POST: Clases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdClase,Tipo,Nombre,FechaHora,IdGimnasio")] Clase clase)
        {
            if (id != clase.IdClase)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(clase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClaseExists(clase.IdClase))
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
            ViewData["IdGimnasio"] = new SelectList(_context.Gimnasio, "Id", "Nombre", clase.IdGimnasio);
            return View(clase);
        }

        // GET: Clases/Delete/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clase = await _context.Clase
                .Include(c => c.Gimnasio)
                .FirstOrDefaultAsync(m => m.IdClase == id);
            if (clase == null)
            {
                return NotFound();
            }

            return View(clase);
        }

        // POST: Clases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clase = await _context.Clase.FindAsync(id);
            if (clase != null)
            {
                _context.Clase.Remove(clase);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClaseExists(int id)
        {
            return _context.Clase.Any(e => e.IdClase == id);
        }
    }
}
