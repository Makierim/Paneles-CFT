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
    public class ProyectosController : Controller
    {
        private readonly PanelesWebContext _context;

        public ProyectosController(PanelesWebContext context)
        {
            _context = context;
        }

        // GET: Proyectos
        public async Task<IActionResult> Index()
        {
            var panelesWebContext = _context.Proyectos.Include(p => p.RunClienteProyectoNavigation).Include(p => p.RunInstaladorProyectoNavigation);
            return View(await panelesWebContext.ToListAsync());
        }

        // GET: Proyectos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyecto = await _context.Proyectos
                .Include(p => p.RunClienteProyectoNavigation)
                .Include(p => p.RunInstaladorProyectoNavigation)
                .FirstOrDefaultAsync(m => m.IdProyecto == id);
            if (proyecto == null)
            {
                return NotFound();
            }

            return View(proyecto);
        }

        // GET: Proyectos/Create
        public IActionResult Create()
        {
            ViewData["RunClienteProyecto"] = new SelectList(_context.Clientes, "RunCliente", "RunCliente");
            ViewData["RunInstaladorProyecto"] = new SelectList(_context.Instaladores, "RunInstalador", "RunInstalador");
            return View();
        }

        // POST: Proyectos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProyecto,RunInstaladorProyecto,RunClienteProyecto,NombreProyecto,FechaInicioProyecto,FechaTerminoProyecto,DireccionProyecto,ResumenDatosProyecto")] Proyecto proyecto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proyecto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RunClienteProyecto"] = new SelectList(_context.Clientes, "RunCliente", "RunCliente", proyecto.RunClienteProyecto);
            ViewData["RunInstaladorProyecto"] = new SelectList(_context.Instaladores, "RunInstalador", "RunInstalador", proyecto.RunInstaladorProyecto);
            return View(proyecto);
        }

        // GET: Proyectos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }
            ViewData["RunClienteProyecto"] = new SelectList(_context.Clientes, "RunCliente", "RunCliente", proyecto.RunClienteProyecto);
            ViewData["RunInstaladorProyecto"] = new SelectList(_context.Instaladores, "RunInstalador", "RunInstalador", proyecto.RunInstaladorProyecto);
            return View(proyecto);
        }

        // POST: Proyectos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProyecto,RunInstaladorProyecto,RunClienteProyecto,NombreProyecto,FechaInicioProyecto,FechaTerminoProyecto,DireccionProyecto,ResumenDatosProyecto")] Proyecto proyecto)
        {
            if (id != proyecto.IdProyecto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proyecto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProyectoExists(proyecto.IdProyecto))
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
            ViewData["RunClienteProyecto"] = new SelectList(_context.Clientes, "RunCliente", "RunCliente", proyecto.RunClienteProyecto);
            ViewData["RunInstaladorProyecto"] = new SelectList(_context.Instaladores, "RunInstalador", "RunInstalador", proyecto.RunInstaladorProyecto);
            return View(proyecto);
        }

        // GET: Proyectos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyecto = await _context.Proyectos
                .Include(p => p.RunClienteProyectoNavigation)
                .Include(p => p.RunInstaladorProyectoNavigation)
                .FirstOrDefaultAsync(m => m.IdProyecto == id);
            if (proyecto == null)
            {
                return NotFound();
            }

            return View(proyecto);
        }

        // POST: Proyectos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                _context.Proyectos.Remove(proyecto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProyectoExists(int id)
        {
            return _context.Proyectos.Any(e => e.IdProyecto == id);
        }
    }
}
