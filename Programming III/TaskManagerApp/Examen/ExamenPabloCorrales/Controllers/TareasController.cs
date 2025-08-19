using ExamenPabloCorrales.Enums;
using ExamenPabloCorrales.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenPabloCorrales.Controllers
{
    public class TareasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TareasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tareas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tarea.Include(t => t.MetaPrincipal);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tareas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tarea
                .Include(t => t.MetaPrincipal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tarea == null)
            {
                return NotFound();
            }

            return View(tarea);
        }

        // GET: Tareas/Create
        public IActionResult Create()
        {
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
          
            ViewData["MetaPrincipalId"] = new SelectList(_context.Set<MetaPrincipal>(), "Id", "Titulo");
            return View();
        }

        // POST: Tareas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,FechaLimite,Estado,Grado,TiempoEstimado,MetaPrincipalId")] Tarea tarea)
        {
            tarea.FechaCreacion = DateTime.Now;
            try
            {
                _context.Add(tarea);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details","Metas", new { id = tarea.MetaPrincipalId });

            }
            catch (Exception ex)
            {

                throw;

            }
        }

        // GET: Tareas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //Carga los valores de los enums para que se puedan selecionar en la vista de edición.
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

            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tarea.FindAsync(id);
            if (tarea == null)
            {
                return NotFound();
            }          

            ViewData["MetaPrincipalId"] = new SelectList(_context.Set<MetaPrincipal>(), "Id", "Titulo", tarea.MetaPrincipalId);

            return View(tarea);
        }

        // POST: Tareas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,FechaCreacion,FechaLimite,Estado,Grado,TiempoEstimado,MetaPrincipalId")] Tarea tarea)
        {
            

            if (id != tarea.Id)
            {
                return NotFound();
            }

            
            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarea);
                    await _context.SaveChangesAsync();
                    await ActualizarEstadoMeta(tarea.MetaPrincipalId);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TareaExists(tarea.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Metas", new { id = tarea.MetaPrincipalId });
            }
            return View(tarea);
        }

        // GET: Tareas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tarea
                .Include(t => t.MetaPrincipal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tarea == null)
            {
                return NotFound();
            }

            return View(tarea);
        }

        // POST: Tareas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tarea = await _context.Tarea.FindAsync(id);
            if (tarea != null)
            {
                _context.Tarea.Remove(tarea);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details","Metas",new { id= tarea.MetaPrincipalId} );
        }

        private bool TareaExists(int id)
        {
            return _context.Tarea.Any(e => e.Id == id);
        }

        private async Task ActualizarEstadoMeta(int metaId)
        {
            var meta = await _context.MetaPrincipal
                .Include(m => m.Tareas)
                .FirstOrDefaultAsync(m => m.Id == metaId);

            if (meta != null && meta.Tareas.Any())
            {
                if (meta.Tareas.All(t => t.Estado == Estado.Completada))
                    meta.Estado = Estado.Completada;
                else
                    meta.Estado = Estado.EnProgreso; 

                _context.Update(meta);
                await _context.SaveChangesAsync();
            }
        }

    }
}
