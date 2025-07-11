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
    public class InstaladoresController : Controller
    {
        private readonly PanelesWebContext _context;

        public InstaladoresController(PanelesWebContext context)
        {
            _context = context;
        }

        // GET: Instaladores
        public async Task<IActionResult> Index()
        {
            var panelesWebContext = _context.Instaladores.Include(i => i.IdRolInstaladorNavigation);
            return View(await panelesWebContext.ToListAsync());
        }

        // GET: Instaladores/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instalador = await _context.Instaladores
                .Include(i => i.IdRolInstaladorNavigation)
                .FirstOrDefaultAsync(m => m.RunInstalador == id);
            if (instalador == null)
            {
                return NotFound();
            }

            return View(instalador);
        }

        // GET: Instaladores/Create
        public IActionResult Create()
        {
            ViewData["IdRolInstalador"] = new SelectList(_context.Roles, "IdRol", "IdRol");
            return View();
        }

        // POST: Instaladores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RunInstalador,UsernameInstalador,NombresInstalador,ApellidosInstalador,DireccionInstalador,TelefonoInstalador,SueldoInstalador,ContrasenaInstalador,IdRolInstalador")] Instalador instalador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(instalador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRolInstalador"] = new SelectList(_context.Roles, "IdRol", "IdRol", instalador.IdRolInstalador);
            return View(instalador);
        }

        // GET: Instaladores/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instalador = await _context.Instaladores.FindAsync(id);
            if (instalador == null)
            {
                return NotFound();
            }
            ViewData["IdRolInstalador"] = new SelectList(_context.Roles, "IdRol", "IdRol", instalador.IdRolInstalador);
            return View(instalador);
        }

        // POST: Instaladores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RunInstalador,UsernameInstalador,NombresInstalador,ApellidosInstalador,DireccionInstalador,TelefonoInstalador,SueldoInstalador,ContrasenaInstalador,IdRolInstalador")] Instalador instalador)
        {
            if (id != instalador.RunInstalador)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instalador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstaladorExists(instalador.RunInstalador))
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
            ViewData["IdRolInstalador"] = new SelectList(_context.Roles, "IdRol", "IdRol", instalador.IdRolInstalador);
            return View(instalador);
        }

        // GET: Instaladores/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instalador = await _context.Instaladores
                .Include(i => i.IdRolInstaladorNavigation)
                .FirstOrDefaultAsync(m => m.RunInstalador == id);
            if (instalador == null)
            {
                return NotFound();
            }

            return View(instalador);
        }

        // POST: Instaladores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var instalador = await _context.Instaladores.FindAsync(id);
            if (instalador != null)
            {
                _context.Instaladores.Remove(instalador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstaladorExists(string id)
        {
            return _context.Instaladores.Any(e => e.RunInstalador == id);
        }
    }
}
