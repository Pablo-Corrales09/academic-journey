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
    public class RutinasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RutinasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rutinas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rutina.ToListAsync());
        }

        // GET: Rutinas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rutina = await _context.Rutina
                .Include(m => m.EjerciciosRutina) // Include Incluye las propiedades de navegación de una entidad, en este caso EjerciciosRutina
                .ThenInclude(m => m.Ejercicio) // ThenInclude, permite incluír las subpropiedades que contenga una entidad ya incluída (con un Include)
                .ThenInclude(m => m.Maquina)
                .Include(m => m.Usuarios)
                .FirstOrDefaultAsync(m => m.IdRutina == id);

            if (rutina == null)
            {
                return NotFound();
            }

            return View(rutina);
        }

        // GET: Rutinas/Create
        [Authorize(Roles = "Administrador,Entrenador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rutinas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Create([Bind("IdRutina,NombreRutina,Nivel,DescripcionRutina")] Rutina rutina)
        {
            try
            {
                _context.Add(rutina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EjercicioRutinasController.Create), "EjercicioRutinas"); //Redirige al usuario a la opción "crear" del modelo EjericioRutinas

            }
            catch (Exception ex)
            {

                throw;

            }
            return View(rutina);
        }

        // GET: Rutinas/Edit/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rutina = await _context.Rutina.FindAsync(id);
            if (rutina == null)
            {
                return NotFound();
            }
            return View(rutina);
        }

        // POST: Rutinas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdRutina,NombreRutina,Nivel,DescripcionRutina")] Rutina rutina)
        {
            if (id != rutina.IdRutina)
            {
                return NotFound();
            }

            try
            {
                _context.Update(rutina);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RutinaExists(rutina.IdRutina))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            return View(rutina);
        }

        // GET: Rutinas/Delete/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rutina = await _context.Rutina
                .FirstOrDefaultAsync(m => m.IdRutina == id);
            if (rutina == null)
            {
                return NotFound();
            }

            return View(rutina);
        }

        // POST: Rutinas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rutina = await _context.Rutina.FindAsync(id);
            if (rutina != null)
            {
                _context.Rutina.Remove(rutina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RutinaExists(int id)
        {
            return _context.Rutina.Any(e => e.IdRutina == id);
        }
    }
}
