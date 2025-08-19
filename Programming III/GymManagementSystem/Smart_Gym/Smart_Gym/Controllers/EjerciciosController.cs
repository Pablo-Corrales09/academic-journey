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
    public class EjerciciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EjerciciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ejercicios
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ejercicio.Include(e => e.Maquina);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ejercicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ejercicio = await _context.Ejercicio
                .Include(e => e.Maquina)
                .FirstOrDefaultAsync(m => m.IdEjercicio == id);
            if (ejercicio == null)
            {
                return NotFound();
            }

            return View(ejercicio);
        }

        // GET: Ejercicios/Create
        [Authorize(Roles = "Administrador,Entrenador")]
        public IActionResult Create()
        {
            ViewData["IdMaquina"] = new SelectList(_context.Maquina, "IdMaquina", "NombreMaquina");
            return View();
        }

        // POST: Ejercicios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Create([Bind("IdEjercicio,NombreEjercicio,DescripcionEjercicio,GrupoMuscular,ImagenEjercicio,IdMaquina")] Ejercicio ejercicio)
        {
            try
            {
                _context.Add(ejercicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                throw;

            }
            ViewData["IdMaquina"] = new SelectList(_context.Maquina, "IdMaquina", "DescripcionMaquina", ejercicio.IdMaquina);
            return View(ejercicio);
        }

        // GET: Ejercicios/Edit/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ejercicio = await _context.Ejercicio.FindAsync(id);
            if (ejercicio == null)
            {
                return NotFound();
            }
            ViewData["IdMaquina"] = new SelectList(_context.Maquina, "IdMaquina", "DescripcionMaquina", ejercicio.IdMaquina);
            return View(ejercicio);
        }

        // POST: Ejercicios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdEjercicio,NombreEjercicio,DescripcionEjercicio,GrupoMuscular,ImagenEjercicio,IdMaquina")] Ejercicio ejercicio)
        {
            if (id != ejercicio.IdEjercicio)
            {
                return NotFound();
            }

            try
            {
                _context.Update(ejercicio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EjercicioExists(ejercicio.IdEjercicio))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            ViewData["IdMaquina"] = new SelectList(_context.Maquina, "IdMaquina", "DescripcionMaquina", ejercicio.IdMaquina);
            return View(ejercicio);
        }

        // GET: Ejercicios/Delete/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ejercicio = await _context.Ejercicio
                .Include(e => e.Maquina)
                .FirstOrDefaultAsync(m => m.IdEjercicio == id);
            if (ejercicio == null)
            {
                return NotFound();
            }

            return View(ejercicio);
        }

        // POST: Ejercicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ejercicio = await _context.Ejercicio.FindAsync(id);
            if (ejercicio != null)
            {
                _context.Ejercicio.Remove(ejercicio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EjercicioExists(int id)
        {
            return _context.Ejercicio.Any(e => e.IdEjercicio == id);
        }
    }
}
