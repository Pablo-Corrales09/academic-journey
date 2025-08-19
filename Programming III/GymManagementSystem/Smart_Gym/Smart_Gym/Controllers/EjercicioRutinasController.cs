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
    [Authorize(Roles = "Administrador,Entrenador")]
    public class EjercicioRutinasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EjercicioRutinasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EjercicioRutinas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.EjercicioRutina.Include(e => e.Ejercicio).Include(e => e.Rutina);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: EjercicioRutinas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ejercicioRutina = await _context.EjercicioRutina
                .Include(e => e.Ejercicio)
                .Include(e => e.Rutina)
                .FirstOrDefaultAsync(m => m.IdEjercicioRutina == id);
            if (ejercicioRutina == null)
            {
                return NotFound();
            }

            return View(ejercicioRutina);
        }

        // GET: EjercicioRutinas/Create
     
        public IActionResult Create()
        {
            ViewData["IdEjercicio"] = new SelectList(_context.Ejercicio, "IdEjercicio", "NombreEjercicio");
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "NombreRutina");
            return View();
        }

        // POST: EjercicioRutinas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public async Task<IActionResult> Create([Bind("IdEjercicioRutina,Repeticiones,Series,IdEjercicio,IdRutina")] EjercicioRutina ejercicioRutina)
        {

            try
            {
                _context.Add(ejercicioRutina);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Rutinas", new { id = ejercicioRutina.IdRutina });

            }
            catch (Exception ex)
            {

                throw;

            }
            ViewData["IdEjercicio"] = new SelectList(_context.Ejercicio, "IdEjercicio", "DescripcionEjercicio", ejercicioRutina.IdEjercicio);
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "DescripcionRutina", ejercicioRutina.IdRutina);
            return View(ejercicioRutina);
        }

        // GET: EjercicioRutinas/Edit/5
      
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ejercicioRutina = await _context.EjercicioRutina.FindAsync(id);
            if (ejercicioRutina == null)
            {
                return NotFound();
            }
            ViewData["IdEjercicio"] = new SelectList(_context.Ejercicio, "IdEjercicio", "NombreEjercicio", ejercicioRutina.IdEjercicio);
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "NombreRutina", ejercicioRutina.IdRutina);
            return View(ejercicioRutina);
        }

        // POST: EjercicioRutinas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public async Task<IActionResult> Edit(int id, [Bind("IdEjercicioRutina,Repeticiones,Series,IdEjercicio,IdRutina")] EjercicioRutina ejercicioRutina)
        {
            if (id != ejercicioRutina.IdEjercicioRutina)
            {
                return NotFound();
            }
            try
            {

                _context.Update(ejercicioRutina);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EjercicioRutinaExists(ejercicioRutina.IdEjercicioRutina))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            ViewData["IdEjercicio"] = new SelectList(_context.Ejercicio, "IdEjercicio", "NombreEjercicio", ejercicioRutina.IdEjercicio);
            ViewData["IdRutina"] = new SelectList(_context.Rutina, "IdRutina", "NombreRutina", ejercicioRutina.IdRutina);

            return RedirectToAction("Details", "Rutinas", new { id = ejercicioRutina.IdRutina });
        }

        // GET: EjercicioRutinas/Delete/5
     
        public async Task<IActionResult> Delete(int? id, int idRutina)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ejercicioRutina = await _context.EjercicioRutina
                .Include(e => e.Ejercicio)
                .Include(e => e.Rutina)
                .FirstOrDefaultAsync(m => m.IdEjercicioRutina == id);
            if (ejercicioRutina == null)
            {
                return NotFound();
            }
            ViewBag.RutinaID = idRutina; // ViewBag permite Pasar el ID de la rutina a la vista
            return View(ejercicioRutina);
        }

        // POST: EjercicioRutinas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> DeleteConfirmed(int id, int idRutina) //Recibe el ID de la rutina de la que se borró el ejercicio
        {
            var ejercicioRutina = await _context.EjercicioRutina.FindAsync(id);
            if (ejercicioRutina != null)
            {
                _context.EjercicioRutina.Remove(ejercicioRutina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Rutinas", new { id = idRutina });
        }

        private bool EjercicioRutinaExists(int id)
        {
            return _context.EjercicioRutina.Any(e => e.IdEjercicioRutina == id);
        }
    }
}

