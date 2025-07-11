using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Paneles_CFT.Models;

namespace Paneles_CFT.Controllers
{
    public class ProyectosComponentesMapsController : Controller
    {
        private readonly PanelesWebContext _context;

        public ProyectosComponentesMapsController(PanelesWebContext context)
        {
            _context = context;
        }

        // GET: ProyectosComponentesMaps
        public async Task<IActionResult> Index()
        {
            var panelesWebContext = _context.ProyectosComponentesMaps.Include(p => p.CodigoComponentePsCsNavigation).Include(p => p.IdProyectoPsCsNavigation);
            return View(await panelesWebContext.ToListAsync());
        }

        // GET: ProyectosComponentesMaps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyectosComponentesMap = await _context.ProyectosComponentesMaps
                .Include(p => p.CodigoComponentePsCsNavigation)
                .Include(p => p.IdProyectoPsCsNavigation)
                .FirstOrDefaultAsync(m => m.IdProyectosComponentes == id);
            if (proyectosComponentesMap == null)
            {
                return NotFound();
            }

            return View(proyectosComponentesMap);
        }

        // GET: ProyectosComponentesMaps/Create
        public IActionResult Create()
        {
            ViewData["CodigoComponentePsCs"] = new SelectList(_context.Componentes, "CodigoComponente", "CodigoComponente");
            ViewData["IdProyectoPsCs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto");
            return View();
        }

        // POST: ProyectosComponentesMaps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProyectosComponentes,IdProyectoPsCs,CodigoComponentePsCs,Cantidad,FechaInstalacionComponente,EstadoUsoComponente")] ProyectosComponentesMap proyectosComponentesMap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proyectosComponentesMap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodigoComponentePsCs"] = new SelectList(_context.Componentes, "CodigoComponente", "CodigoComponente", proyectosComponentesMap.CodigoComponentePsCs);
            ViewData["IdProyectoPsCs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto", proyectosComponentesMap.IdProyectoPsCs);
            return View(proyectosComponentesMap);
        }

        // GET: ProyectosComponentesMaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyectosComponentesMap = await _context.ProyectosComponentesMaps.FindAsync(id);
            if (proyectosComponentesMap == null)
            {
                return NotFound();
            }
            ViewData["CodigoComponentePsCs"] = new SelectList(_context.Componentes, "CodigoComponente", "CodigoComponente", proyectosComponentesMap.CodigoComponentePsCs);
            ViewData["IdProyectoPsCs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto", proyectosComponentesMap.IdProyectoPsCs);
            return View(proyectosComponentesMap);
        }

        // POST: ProyectosComponentesMaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProyectosComponentes,IdProyectoPsCs,CodigoComponentePsCs,Cantidad,FechaInstalacionComponente,EstadoUsoComponente")] ProyectosComponentesMap proyectosComponentesMap)
        {
            if (id != proyectosComponentesMap.IdProyectosComponentes)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proyectosComponentesMap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProyectosComponentesMapExists(proyectosComponentesMap.IdProyectosComponentes))
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
            ViewData["CodigoComponentePsCs"] = new SelectList(_context.Componentes, "CodigoComponente", "CodigoComponente", proyectosComponentesMap.CodigoComponentePsCs);
            ViewData["IdProyectoPsCs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto", proyectosComponentesMap.IdProyectoPsCs);
            return View(proyectosComponentesMap);
        }

        // GET: ProyectosComponentesMaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyectosComponentesMap = await _context.ProyectosComponentesMaps
                .Include(p => p.CodigoComponentePsCsNavigation)
                .Include(p => p.IdProyectoPsCsNavigation)
                .FirstOrDefaultAsync(m => m.IdProyectosComponentes == id);
            if (proyectosComponentesMap == null)
            {
                return NotFound();
            }

            return View(proyectosComponentesMap);
        }

        // POST: ProyectosComponentesMaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyectosComponentesMap = await _context.ProyectosComponentesMaps.FindAsync(id);
            if (proyectosComponentesMap != null)
            {
                _context.ProyectosComponentesMaps.Remove(proyectosComponentesMap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProyectosComponentesMapExists(int id)
        {
            return _context.ProyectosComponentesMaps.Any(e => e.IdProyectosComponentes == id);
        }
    }
}
