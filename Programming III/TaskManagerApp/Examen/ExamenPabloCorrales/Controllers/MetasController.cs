using ExamenPabloCorrales.Enums;
using ExamenPabloCorrales.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenPabloCorrales.Controllers
{
    public class MetasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MetasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Metas
        public async Task<IActionResult> Index()
        {
            return View(await _context.MetaPrincipal.ToListAsync());
        }

        // GET: Metas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaPrincipal = await _context.MetaPrincipal
                .Include(m => m.Tareas) // Incluye las tareas relacionadas con la meta
                .FirstOrDefaultAsync(m => m.Id == id);
            if (metaPrincipal == null)
            {
                return NotFound();
            }

            return View(metaPrincipal);
        }

        // GET: Metas/Create
        public IActionResult Create() {

            // Cargar los valores de los enums en ViewBag para usarlos en el formulario
            
            //Declara una "categorias" que obtiene un array con los valores del enum Categoria y aplica ToString() para convertirlos a string.
            var categorias = Enum.GetValues(typeof(ExamenPabloCorrales.Enums.Categoria))
                .Cast<ExamenPabloCorrales.Enums.Categoria>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString()
                }).ToList();

            ViewBag.Categorias = categorias;

            var grados = Enum.GetValues(typeof(ExamenPabloCorrales.Enums.Grado))
                .Cast<ExamenPabloCorrales.Enums.Grado>()
                .Select(g => new SelectListItem
                {
                    Value = g.ToString(),
                    Text = g.ToString()
                }).ToList();

            ViewBag.Grados = grados;

            var estados = Enum.GetValues(typeof(ExamenPabloCorrales.Enums.Estado))
                .Cast<ExamenPabloCorrales.Enums.Estado>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                }).ToList();

            ViewBag.Estados = estados;

            return View();
        }

        // POST: Metas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Descripcion,Categoria,FechaCreacion,FechaLimite,Grado,Estado")] MetaPrincipal metaPrincipal)
        {
            metaPrincipal.FechaCreacion = DateTime.Now; // Asigna la fecha actual como fecha de creación de manera predeterminada
            if (ModelState.IsValid)
            {
                _context.Add(metaPrincipal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(metaPrincipal);
        }

        // GET: Metas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaPrincipal = await _context.MetaPrincipal.FindAsync(id);
            if (metaPrincipal == null)
            {
                return NotFound();
            }
            //Carga los valores de los enums para que se puedan selecionar en la vista de edición.
            var categorias = Enum.GetValues(typeof(ExamenPabloCorrales.Enums.Categoria))
                .Cast<ExamenPabloCorrales.Enums.Categoria>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString()
                }).ToList();

            ViewBag.Categorias = categorias;

            var grados = Enum.GetValues(typeof(ExamenPabloCorrales.Enums.Grado))
                .Cast<ExamenPabloCorrales.Enums.Grado>()
                .Select(g => new SelectListItem
                {
                    Value = g.ToString(),
                    Text = g.ToString()
                }).ToList();

            ViewBag.Grados = grados;

            var estados = Enum.GetValues(typeof(ExamenPabloCorrales.Enums.Estado))
                .Cast<ExamenPabloCorrales.Enums.Estado>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                }).ToList();

            ViewBag.Estados = estados;

            return View(metaPrincipal);
        }

        // POST: Metas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descripcion,Categoria,FechaCreacion,FechaLimite,Grado,Estado")] MetaPrincipal metaPrincipal)
        {
            if (id != metaPrincipal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(metaPrincipal);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MetaPrincipalExists(metaPrincipal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details","Metas",new { id= metaPrincipal.Id} );
            }
            return View(metaPrincipal);
        }

        // GET: Metas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaPrincipal = await _context.MetaPrincipal
                .FirstOrDefaultAsync(m => m.Id == id);
            if (metaPrincipal == null)
            {
                return NotFound();
            }

            return View(metaPrincipal);
        }

        // POST: Metas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var metaPrincipal = await _context.MetaPrincipal.FindAsync(id);
            if (metaPrincipal != null)
            {
                _context.MetaPrincipal.Remove(metaPrincipal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MetaPrincipalExists(int id)
        {
            return _context.MetaPrincipal.Any(e => e.Id == id);
        }

        

    }
}
