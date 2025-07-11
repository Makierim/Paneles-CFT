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
    public class ProyectosNormativasMapsController : Controller
    {
        private readonly PanelesWebContext _context;

        public ProyectosNormativasMapsController(PanelesWebContext context)
        {
            _context = context;
        }

        // GET: ProyectosNormativasMaps
        public async Task<IActionResult> Index()
        {
            var panelesWebContext = _context.ProyectosNormativasMaps.Include(p => p.CodigoNormativaPsNsNavigation).Include(p => p.IdProyectoPsNsNavigation);
            return View(await panelesWebContext.ToListAsync());
        }

        // GET: ProyectosNormativasMaps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyectosNormativasMap = await _context.ProyectosNormativasMaps
                .Include(p => p.CodigoNormativaPsNsNavigation)
                .Include(p => p.IdProyectoPsNsNavigation)
                .FirstOrDefaultAsync(m => m.IdProyectosNormativas == id);
            if (proyectosNormativasMap == null)
            {
                return NotFound();
            }

            return View(proyectosNormativasMap);
        }

        // GET: ProyectosNormativasMaps/Create
        public IActionResult Create()
        {
            ViewData["CodigoNormativaPsNs"] = new SelectList(_context.Normativas, "CodigoNormativa", "CodigoNormativa");
            ViewData["IdProyectoPsNs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto");
            return View();
        }

        // POST: ProyectosNormativasMaps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProyectosNormativas,IdProyectoPsNs,CodigoNormativaPsNs,EsObligatoria")] ProyectosNormativasMap proyectosNormativasMap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proyectosNormativasMap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodigoNormativaPsNs"] = new SelectList(_context.Normativas, "CodigoNormativa", "CodigoNormativa", proyectosNormativasMap.CodigoNormativaPsNs);
            ViewData["IdProyectoPsNs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto", proyectosNormativasMap.IdProyectoPsNs);
            return View(proyectosNormativasMap);
        }

        // GET: ProyectosNormativasMaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyectosNormativasMap = await _context.ProyectosNormativasMaps.FindAsync(id);
            if (proyectosNormativasMap == null)
            {
                return NotFound();
            }
            ViewData["CodigoNormativaPsNs"] = new SelectList(_context.Normativas, "CodigoNormativa", "CodigoNormativa", proyectosNormativasMap.CodigoNormativaPsNs);
            ViewData["IdProyectoPsNs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto", proyectosNormativasMap.IdProyectoPsNs);
            return View(proyectosNormativasMap);
        }

        // POST: ProyectosNormativasMaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProyectosNormativas,IdProyectoPsNs,CodigoNormativaPsNs,EsObligatoria")] ProyectosNormativasMap proyectosNormativasMap)
        {
            if (id != proyectosNormativasMap.IdProyectosNormativas)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proyectosNormativasMap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProyectosNormativasMapExists(proyectosNormativasMap.IdProyectosNormativas))
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
            ViewData["CodigoNormativaPsNs"] = new SelectList(_context.Normativas, "CodigoNormativa", "CodigoNormativa", proyectosNormativasMap.CodigoNormativaPsNs);
            ViewData["IdProyectoPsNs"] = new SelectList(_context.Proyectos, "IdProyecto", "IdProyecto", proyectosNormativasMap.IdProyectoPsNs);
            return View(proyectosNormativasMap);
        }

        // GET: ProyectosNormativasMaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proyectosNormativasMap = await _context.ProyectosNormativasMaps
                .Include(p => p.CodigoNormativaPsNsNavigation)
                .Include(p => p.IdProyectoPsNsNavigation)
                .FirstOrDefaultAsync(m => m.IdProyectosNormativas == id);
            if (proyectosNormativasMap == null)
            {
                return NotFound();
            }

            return View(proyectosNormativasMap);
        }

        // POST: ProyectosNormativasMaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyectosNormativasMap = await _context.ProyectosNormativasMaps.FindAsync(id);
            if (proyectosNormativasMap != null)
            {
                _context.ProyectosNormativasMaps.Remove(proyectosNormativasMap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProyectosNormativasMapExists(int id)
        {
            return _context.ProyectosNormativasMaps.Any(e => e.IdProyectosNormativas == id);
        }
    }
}
