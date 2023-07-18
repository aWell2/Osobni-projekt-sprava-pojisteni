using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projekt.Data;
using Projekt.Models;
using System.Data;

namespace Projekt.Controllers
{
	[Authorize(Roles = "admin, User")]
	public class InsurenceEventsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public InsurenceEventsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: InsurenceEvents
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber)
		{

			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			ViewBag.CurrentFilter = searchString;

			var insurenceEvents = from I in _context.InsurenceEvent.Include(E => E.Insurence.InsurenceHolder)
								  select I;


			if (User.IsInRole("User"))
			{
				insurenceEvents = insurenceEvents.Where(I => I.Insurence.InsurenceHolder.Email == User.Identity.Name);
			}
			if (!searchString.IsNullOrEmpty())
			{

				insurenceEvents = insurenceEvents.Where(I => (I.Insurence.InsurenceHolder.FirstName + " " + I.Insurence.InsurenceHolder.LastName).Contains(searchString)
														  || (I.Insurence.InsurenceHolder.LastName + " " + I.Insurence.InsurenceHolder.FirstName).Contains(searchString));
			}

			int pageSize = 10;
			return View(await PaginatedList<InsurenceEvent>.CreateAsync(insurenceEvents, pageNumber ?? 1, pageSize));

		}

		// GET: InsurenceEvents/Details/5
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Details(int? id, int? insurenceRouteId)
		{
			if (id == null || _context.InsurenceEvent == null)
			{
				return NotFound();
			}

			var insurenceEvent = await _context.InsurenceEvent
				.Include(i => i.Insurence.InsurenceHolder)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (insurenceEvent == null)
			{
				return NotFound();
			}
			// zajistí aby uživatel nemohl přejít na ostatní položky přes url
			if (User.IsInRole("User") && (insurenceEvent.Insurence.InsurenceHolder.Email != User.Identity.Name))
			{
				TempData["error"] = "Přístup zamítnut";
				return RedirectToAction("Index");
			}

			if (insurenceRouteId != null)
			{
				ViewBag.insurenceRouteId = insurenceRouteId;
			}

			return View(insurenceEvent);
		}

		// GET: InsurenceEvents/Create
		[Authorize(Roles = "admin")]
		public IActionResult Create(int? insurenceId)
		{
			// vytvoří instanci třídy aby datum už bylo přednastaveno
			InsurenceEvent insurenceEvent = new InsurenceEvent();
			if (insurenceId != null)
			{
				// Do ViewBagu vloží pojištění, ke kterému přidáváme pojistnou událost
				ViewBag.Insurence = _context.Insurence.Where(I => I.Id == insurenceId).First();

			}
			else
			{
				ViewData["InsurenceId"] = new SelectList(_context.Insurence.Select(I => new
				{
					Id = I.Id,
					DisplayInfo = $"{I.TypeOfInsurence}: {I.InsurenceHolder.FirstName} {I.InsurenceHolder.LastName}"
				}), "Id", "DisplayInfo");

			}
			return View(insurenceEvent);
		}

		// POST: InsurenceEvents/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Description,TimeOfEvent,PlaceOfEvent,InsurenceId")] InsurenceEvent insurenceEvent, int? insurenceRouteId)
		{
			if (ModelState.IsValid)
			{
				_context.Add(insurenceEvent);
				// Vybere pojištění, ke kterému byla přidána pojistná událost
				var insurence = _context.Insurence.Where(i => i.Id == insurenceEvent.InsurenceId).First();
				// Zvýší počet pojistných událostí pojištění
				insurence.InsurenceEventsCount++;
				await _context.SaveChangesAsync();

				TempData["success"] = "Událost úspěšně vytvořena";
				// Pokud tvoříme pojistnou událost přes pojištění, tak vrátí pohled pojištění
				if (insurenceRouteId != null)
				{
					return RedirectToAction("Details", "Insurences", new { id = insurence.Id });
				}
				else
					return RedirectToAction(nameof(Index));
			}
			ViewData["InsurenceId"] = new SelectList(_context.Insurence.Select(I => new
			{
				Id = I.Id,
				DisplayInfo = $"{I.TypeOfInsurence}: {I.InsurenceHolder.FirstName} {I.InsurenceHolder.LastName}"
			}), "Id", "DisplayInfo");
			return View(insurenceEvent);
		}

		// GET: InsurenceEvents/Edit/5
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> Edit(int? id, int? insurenceRouteId)
		{
			if (id == null || _context.InsurenceEvent == null)
			{
				return NotFound();
			}

			var insurenceEvent = await _context.InsurenceEvent.FindAsync(id);
			if (insurenceEvent == null)
			{
				return NotFound();
			}
			if (insurenceRouteId != null)
			{
				ViewBag.insurenceRouteId = insurenceRouteId;
			}
			ViewData["InsurenceId"] = new SelectList(_context.Insurence.Select(I => new
			{
				Id = I.Id,
				DisplayInfo = $"{I.TypeOfInsurence}: {I.InsurenceHolder.FirstName} {I.InsurenceHolder.LastName}"
			}), "Id", "DisplayInfo");
			return View(insurenceEvent);
		}

		// POST: InsurenceEvents/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Description,TimeOfEvent,PlaceOfEvent,InsurenceId")] InsurenceEvent insurenceEvent, int? insurenceRouteId)
		{
			if (id != insurenceEvent.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(insurenceEvent);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!InsurenceEventExists(insurenceEvent.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				TempData["success"] = "Změny uloženy";
				if (insurenceRouteId != null)
				{
					return RedirectToAction("Details", "Insurences", new { id = insurenceEvent.InsurenceId });
				}

				return RedirectToAction(nameof(Index));
			}
			ViewData["InsurenceId"] = new SelectList(_context.Insurence.Select(I => new
			{
				Id = I.Id,
				DisplayInfo = $"{I.TypeOfInsurence}: {I.InsurenceHolder.FirstName} {I.InsurenceHolder.LastName}"
			}), "Id", "DisplayInfo");
			return View(insurenceEvent);
		}

		// GET: InsurenceEvents/Delete/5
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> Delete(int? id, int? insurenceRouteId)
		{
			if (id == null || _context.InsurenceEvent == null)
			{
				return NotFound();
			}

			var insurenceEvent = await _context.InsurenceEvent
				.Include(i => i.Insurence.InsurenceHolder)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (insurenceEvent == null)
			{
				return NotFound();
			}
			if (insurenceRouteId != null)
			{
				ViewBag.insurenceRouteId = insurenceRouteId;
			}


			return View(insurenceEvent);
		}

		// POST: InsurenceEvents/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id, int? insurenceRouteId)
		{
			if (_context.InsurenceEvent == null)
			{
				return Problem("Entity set 'ApplicationDbContext.InsurenceEvent'  is null.");
			}
			var insurenceEvent = await _context.InsurenceEvent.FindAsync(id);
			// pomocná proměnná pro routing
			int? insurenceId = null;
			if (insurenceEvent != null)
			{
				// sníží pojistění počet událostí
				var insurence = _context.Insurence.Where(i => i.Id == insurenceEvent.InsurenceId).First();
				insurence.InsurenceEventsCount--;
				insurenceId = insurence.Id;

				_context.InsurenceEvent.Remove(insurenceEvent);
			}

			await _context.SaveChangesAsync();
			TempData["warning"] = "Událost úspěšně vymazána";
			//pokud mažeme událost řes pojištění tak se vrátíme na jeho pohled
			if (insurenceRouteId != null)
			{
				return RedirectToAction("Details", "Insurences", new { id = insurenceId });
			}
			return RedirectToAction(nameof(Index));
		}

		private bool InsurenceEventExists(int id)
		{
			return (_context.InsurenceEvent?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
