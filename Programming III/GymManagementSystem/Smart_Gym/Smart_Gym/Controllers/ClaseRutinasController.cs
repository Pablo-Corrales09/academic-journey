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
    public class ClaseRutinasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClaseRutinasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ClaseRutinas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClaseRutina.Include(c => c.Clase).Include(c => c.Rutina);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClaseRutinas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claseRutina = await _context.ClaseRutina
                .Include(c => c.Clase)
                .Include(c => c.Rutina)
                .FirstOrDefaultAsync(m => m.IdClaseRutina == id);
            if (claseRutina == null)
            {
                return NotFound();
            }

            return View(claseRutina);
        }

        // GET: ClaseRutinas/Create
        [Authorize(Roles = "Administrador,Entrenador")]
        public IActionResult Create()
        {
            ViewData["IdClase"] = new SelectList(_context.Clase, "IdClase", "Nombre");
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "DescripcionRutina");
            return View();
        }

        // POST: ClaseRutinas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Create([Bind("IdClaseRutina,IdClase,IdRutina")] ClaseRutina claseRutina)
        {
            try
            {
                _context.Add(claseRutina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                throw;

            }
            ViewData["IdClase"] = new SelectList(_context.Clase, "IdClase", "Nombre", claseRutina.IdClase);
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "DescripcionRutina", claseRutina.IdRutina);
            return View(claseRutina);
        }

        // GET: ClaseRutinas/Edit/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claseRutina = await _context.ClaseRutina.FindAsync(id);
            if (claseRutina == null)
            {
                return NotFound();
            }
            ViewData["IdClase"] = new SelectList(_context.Clase, "IdClase", "Nombre", claseRutina.IdClase);
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "DescripcionRutina", claseRutina.IdRutina);
            return View(claseRutina);
        }

        // POST: ClaseRutinas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdClaseRutina,IdClase,IdRutina")] ClaseRutina claseRutina)
        {
            if (id != claseRutina.IdClaseRutina)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(claseRutina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClaseRutinaExists(claseRutina.IdClaseRutina))
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
            ViewData["IdClase"] = new SelectList(_context.Clase, "IdClase", "Nombre", claseRutina.IdClase);
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "DescripcionRutina", claseRutina.IdRutina);
            return View(claseRutina);
        }

        // GET: ClaseRutinas/Delete/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claseRutina = await _context.ClaseRutina
                .Include(c => c.Clase)
                .Include(c => c.Rutina)
                .FirstOrDefaultAsync(m => m.IdClaseRutina == id);
            if (claseRutina == null)
            {
                return NotFound();
            }

            return View(claseRutina);
        }

        // POST: ClaseRutinas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claseRutina = await _context.ClaseRutina.FindAsync(id);
            if (claseRutina != null)
            {
                _context.ClaseRutina.Remove(claseRutina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClaseRutinaExists(int id)
        {
            return _context.ClaseRutina.Any(e => e.IdClaseRutina == id);
        }
    }
}
