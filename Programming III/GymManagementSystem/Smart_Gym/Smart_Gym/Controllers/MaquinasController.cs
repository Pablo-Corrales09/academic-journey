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
    public class MaquinasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaquinasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Maquinas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Maquina.ToListAsync());
        }

        // GET: Maquinas/Details/5
        public async Task<IActionResult> Details(int? id, string regresar, int? idRutina)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maquina = await _context.Maquina
                .FirstOrDefaultAsync(m => m.IdMaquina == id);
            if (maquina == null)
            {
                return NotFound();
            }
            ViewBag.IdRutina = idRutina;
            ViewBag.Regresar = regresar;
            return View(maquina);
        }

        // GET: Maquinas/Create
        [Authorize(Roles = "Administrador,Entrenador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Maquinas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Create([Bind("IdMaquina,NombreMaquina,DescripcionMaquina,ImagenMaquina")] Maquina maquina)
        {
            try
            {
                _context.Add(maquina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                throw;

            }
            return View(maquina);
        }

        // GET: Maquinas/Edit/5
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maquina = await _context.Maquina.FindAsync(id);
            if (maquina == null)
            {
                return NotFound();
            }
            return View(maquina);
        }

        // POST: Maquinas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Entrenador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdMaquina,NombreMaquina,DescripcionMaquina,ImagenMaquina")] Maquina maquina)
        {
            if (id != maquina.IdMaquina)
            {
                return NotFound();
            }
            try
            {
                _context.Update(maquina);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaquinaExists(maquina.IdMaquina))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            return View(maquina);
        }

        // GET: Maquinas/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maquina = await _context.Maquina
                .FirstOrDefaultAsync(m => m.IdMaquina == id);
            if (maquina == null)
            {
                return NotFound();
            }

            return View(maquina);
        }

        // POST: Maquinas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maquina = await _context.Maquina.FindAsync(id);
            if (maquina != null)
            {
                _context.Maquina.Remove(maquina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaquinaExists(int id)
        {
            return _context.Maquina.Any(e => e.IdMaquina == id);
        }
    }
}
