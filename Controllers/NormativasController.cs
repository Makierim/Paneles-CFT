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
    public class NormativasController : Controller
    {
        private readonly PanelesWebContext _context;

        public NormativasController(PanelesWebContext context)
        {
            _context = context;
        }

        // GET: Normativas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Normativas.ToListAsync());
        }

        // GET: Normativas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var normativa = await _context.Normativas
                .FirstOrDefaultAsync(m => m.CodigoNormativa == id);
            if (normativa == null)
            {
                return NotFound();
            }

            return View(normativa);
        }

        // GET: Normativas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Normativas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodigoNormativa,TituloNormativa,DescripcionUrlNormativa,TipoNormativa,RegionNormativa,ComunaNormativa,InstitucionNormativa,FechaPublicacionNormativa,FechaInicioVigencia,FechaFinVigencia")] Normativa normativa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(normativa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(normativa);
        }

        // GET: Normativas/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var normativa = await _context.Normativas.FindAsync(id);
            if (normativa == null)
            {
                return NotFound();
            }
            return View(normativa);
        }

        // POST: Normativas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodigoNormativa,TituloNormativa,DescripcionUrlNormativa,TipoNormativa,RegionNormativa,ComunaNormativa,InstitucionNormativa,FechaPublicacionNormativa,FechaInicioVigencia,FechaFinVigencia")] Normativa normativa)
        {
            if (id != normativa.CodigoNormativa)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(normativa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NormativaExists(normativa.CodigoNormativa))
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
            return View(normativa);
        }

        // GET: Normativas/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var normativa = await _context.Normativas
                .FirstOrDefaultAsync(m => m.CodigoNormativa == id);
            if (normativa == null)
            {
                return NotFound();
            }

            return View(normativa);
        }

        // POST: Normativas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var normativa = await _context.Normativas.FindAsync(id);
            if (normativa != null)
            {
                _context.Normativas.Remove(normativa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NormativaExists(string id)
        {
            return _context.Normativas.Any(e => e.CodigoNormativa == id);
        }
    }
}
