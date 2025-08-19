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
    public class UsuarioClasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuarioClasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UsuarioClases
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UsuarioClase
                .Include(u => u.ClaseRutina)
                .ThenInclude(cr => cr.Clase) //Con esta propiedad Entity Framework incluirá la Clase asociada a la ClaseRutina
                .Include(u => u.Usuario);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UsuarioClases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioClase = await _context.UsuarioClase
                .Include(u => u.ClaseRutina)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.IdUsuarioClase == id);
            if (usuarioClase == null)
            {
                return NotFound();
            }

            return View(usuarioClase);
        }

        // GET: UsuarioClases/Create
        [Authorize(Roles = "Administrador,Entrenador,Cliente")]
        public IActionResult Create()
        {
            // Por medio del Include se puede acceder a Clase desde ClaseRutina y obtener el nombre.
            ViewData["IdClaseRutina"] = new SelectList(_context.ClaseRutina.Include(cr =>cr.Clase).ToList(),
                "IdClaseRutina", "Clase.Nombre");

            //Se crea una lista por medio de .Select para mostrar el nombre del usuario completo en las opciones del dropdown.
            ViewData["IdUsuario"] = new SelectList(_context.Users
                .Select(u=> new { Id = u.Id, NombreCompleto = u.Nombre + " " + u.Apellido }).ToList(), "Id", "NombreCompleto");
            return View();
        }

        // POST: UsuarioClases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador,Cliente")]
        public async Task<IActionResult> Create([Bind("IdUsuarioClase,IdUsuario,IdClaseRutina")] UsuarioClase usuarioClase)
        {
            try
            {
                _context.Add(usuarioClase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                throw;

            }
            ViewData["IdClaseRutina"] = new SelectList(_context.ClaseRutina, "IdClaseRutina", "IdClaseRutina", usuarioClase.IdClaseRutina);
            ViewData["IdUsuario"] = new SelectList(_context.Users, "Id", "Id", usuarioClase.IdUsuario);
            return View(usuarioClase);
        }

        // GET: UsuarioClases/Edit/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioClase = await _context.UsuarioClase.FindAsync(id);
            if (usuarioClase == null)
            {
                return NotFound();
            }
            ViewData["IdClaseRutina"] = new SelectList(_context.ClaseRutina, "IdClaseRutina", "IdClaseRutina", usuarioClase.IdClaseRutina);
            ViewData["IdUsuario"] = new SelectList(_context.Users, "Id", "Id", usuarioClase.IdUsuario);
            return View(usuarioClase);
        }

        // POST: UsuarioClases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuarioClase,IdUsuario,IdClaseRutina")] UsuarioClase usuarioClase)
        {
            if (id != usuarioClase.IdUsuarioClase)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarioClase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioClaseExists(usuarioClase.IdUsuarioClase))
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
            ViewData["IdClaseRutina"] = new SelectList(_context.ClaseRutina, "IdClaseRutina", "IdClaseRutina", usuarioClase.IdClaseRutina);
            ViewData["IdUsuario"] = new SelectList(_context.Users, "Id", "Id", usuarioClase.IdUsuario);
            return View(usuarioClase);
        }

        // GET: UsuarioClases/Delete/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioClase = await _context.UsuarioClase
                .Include(u => u.ClaseRutina)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.IdUsuarioClase == id);
            if (usuarioClase == null)
            {
                return NotFound();
            }

            return View(usuarioClase);
        }

        // POST: UsuarioClases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioClase = await _context.UsuarioClase.FindAsync(id);
            if (usuarioClase != null)
            {
                _context.UsuarioClase.Remove(usuarioClase);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioClaseExists(int id)
        {
            return _context.UsuarioClase.Any(e => e.IdUsuarioClase == id);
        }
    }
}
