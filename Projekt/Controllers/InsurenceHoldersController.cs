using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Data;
using Projekt.Models;

namespace Projekt.Controllers
{
	[Authorize(Roles = "admin, User")]
	public class InsurenceHoldersController : Controller
	{
		private readonly ApplicationDbContext _context;

		public InsurenceHoldersController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: InsurenceHolders
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Index(string? sortOrder, string searchString, string currentFilter, int? pageNumber)
		{
			if (_context.InsurenceHolder != null)
			{
				ViewBag.CurrentSort = sortOrder;
				ViewBag.NameSort = sortOrder == "nameAsc" ? "nameDesc" : "nameAsc";
				ViewBag.CountSort = sortOrder == "countDesc" ? "countAsc" : "countDesc";

				if (searchString != null)
				{
					pageNumber = 1;
				}
				else
				{
					searchString = currentFilter;
				}
				ViewBag.CurrentFilter = searchString;

				var insurenceHolder = from I in _context.InsurenceHolder
									  select I;
				if (!string.IsNullOrEmpty(searchString))
				{
					insurenceHolder = insurenceHolder.Where(I => (I.FirstName + " " + I.LastName).Contains(searchString)
															  || (I.LastName + " " + I.FirstName).Contains(searchString));
				}

				if (User.IsInRole("User"))
				{
					insurenceHolder = insurenceHolder.Where(I => I.Email == User.Identity.Name);
				}

				switch (sortOrder)
				{
					case "nameDesc":
						insurenceHolder = insurenceHolder.OrderByDescending(I => I.LastName);
						break;
					case "nameAsc":
						insurenceHolder = insurenceHolder.OrderBy(I => I.LastName);
						break;
					case "countDesc":
						insurenceHolder = insurenceHolder.OrderByDescending(I => I.InsurenceCount);
						break;
					case "countAsc":
						insurenceHolder = insurenceHolder.OrderBy(I => I.InsurenceCount);
						break;

				}
				int pageSize = 2;
				return View(await PaginatedList<InsurenceHolder>.CreateAsync(insurenceHolder, pageNumber ?? 1, pageSize));
			}

			return Problem("Entity set 'ApplicationDbContext.InsurenceHolder'  is null.");


		}

		// GET: InsurenceHolders/Details/5
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.InsurenceHolder == null)
			{
				return NotFound();
			}

			var insurenceHolder = await _context.InsurenceHolder.Include(I => I.Insurences)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (insurenceHolder == null)
			{
				return NotFound();
			}
			// zajistí aby uživatel nemohl přejít na ostatní položky přes url
			if (User.IsInRole("User") && (insurenceHolder.Email != User.Identity.Name))
			{
				TempData["error"] = "Přístup zamítnut";
				return RedirectToAction("Index");
			}
			return View(insurenceHolder);
		}
		[Authorize(Roles = "admin")]
		// GET: InsurenceHolders/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: InsurenceHolders/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,Email,Street,City,PostalCode")] InsurenceHolder insurenceHolder)
		{
			if (ModelState.IsValid)
			{
				_context.Add(insurenceHolder);
				await _context.SaveChangesAsync();
				TempData["success"] = "Pojistník úspěšně vytvořen";
				return RedirectToAction(nameof(Index));
			}
			return View(insurenceHolder);
		}

		// GET: InsurenceHolders/Edit/5
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.InsurenceHolder == null)
			{
				return NotFound();
			}

			var insurenceHolder = await _context.InsurenceHolder.FindAsync(id);
			if (insurenceHolder == null)
			{
				return NotFound();
			}
			return View(insurenceHolder);
		}

		// POST: InsurenceHolders/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,PhoneNumber,Email,Street,City,PostalCode,InsurenceCount")] InsurenceHolder insurenceHolder)
		{
			if (id != insurenceHolder.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(insurenceHolder);
					await _context.SaveChangesAsync();
					TempData["success"] = "Změny uloženy";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!InsurenceHolderExists(insurenceHolder.Id))
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
			return View(insurenceHolder);
		}

		// GET: InsurenceHolders/Delete/5
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.InsurenceHolder == null)
			{
				return NotFound();
			}

			var insurenceHolder = await _context.InsurenceHolder
				.FirstOrDefaultAsync(m => m.Id == id);
			if (insurenceHolder == null)
			{
				return NotFound();
			}

			return View(insurenceHolder);
		}

		// POST: InsurenceHolders/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.InsurenceHolder == null)
			{
				return Problem("Entity set 'ApplicationDbContext.InsurenceHolder'  is null.");
			}
			var insurenceHolder = await _context.InsurenceHolder.FindAsync(id);
			if (insurenceHolder != null)
			{
				var insurences = _context.Insurence.Where(Insurence => Insurence.Id == id).ToList();
				// odstraní pojisky a jejich události
				foreach (var I in insurences)
				{
					var events = _context.InsurenceEvent.Where(e => e.InsurenceId == I.Id);
					_context.InsurenceEvent.RemoveRange(events);
					_context.Insurence.Remove(I);
				}

				_context.InsurenceHolder.Remove(insurenceHolder);
			}

			await _context.SaveChangesAsync();
			TempData["warning"] = "Pojistník úspěšně odstraněn";
			return RedirectToAction(nameof(Index));
		}

		private bool InsurenceHolderExists(int id)
		{
			return (_context.InsurenceHolder?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
