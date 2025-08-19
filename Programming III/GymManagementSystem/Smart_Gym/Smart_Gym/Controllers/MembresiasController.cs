using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Smart_Gym.Data;
using Smart_Gym.Models;

namespace Smart_Gym.Controllers
{
    public class MembresiasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembresiasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Membresias
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Membresia.Include(m => m.Cliente);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Membresias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membresia = await _context.Membresia
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membresia == null)
            {
                return NotFound();
            }

            return View(membresia);
        }

        // GET: Membresias/Create
        public IActionResult Create()
        {
            var clientes = _context.Users
                .Select(c => new
                {
                    c.Id,
                    NombreCompleto = c.Nombre + " " + c.Apellido
                })
                .ToList();

            ViewData["ClienteId"] = new SelectList(clientes, "Id", "NombreCompleto");
            return View();
        }

        // POST: Membresias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,Tipo,FechaInicio,FechaExpiracion,EstaPagada")] Membresia membresia)
        {
            try
            {
                _context.Add(membresia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FacturasController.Create), "Facturas");

            }
            catch (Exception ex)
            {

                throw;

            }            

            return View(membresia);
        }

        // GET: Membresias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membresia = await _context.Membresia.FindAsync(id);
            if (membresia == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Id", membresia.ClienteId);
            return View(membresia);
        }

        // POST: Membresias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Tipo,FechaInicio,FechaExpiracion,EstaPagada")] Membresia membresia)
        {
            if (id != membresia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membresia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembresiaExists(membresia.Id))
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
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Id", membresia.ClienteId);
            return View(membresia);
        }

        // GET: Membresias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membresia = await _context.Membresia
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membresia == null)
            {
                return NotFound();
            }

            return View(membresia);
        }

        // POST: Membresias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var membresia = await _context.Membresia.FindAsync(id);
            if (membresia != null)
            {
                _context.Membresia.Remove(membresia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MembresiaExists(int id)
        {
            return _context.Membresia.Any(e => e.Id == id);
        }
    }
}
