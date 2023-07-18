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
	public class InsurencesController : Controller
	{
		private readonly ApplicationDbContext _context;
		public InsurencesController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Insurences
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Index(string? sortOrder, string searchString, string currentFilter, int? pageNumber, string dropdownSearchString)
		{
			ViewBag.CurrentSort = sortOrder;
			ViewBag.DateSort = sortOrder == "dateDesc" ? "dateAsc" : "dateDesc";
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
			ViewBag.DropdownSearchString = dropdownSearchString;

			var insurences = from I in _context.Insurence.Include(H => H.InsurenceHolder)
							 select I;


			if (User.IsInRole("User"))
			{
				insurences = insurences.Where(I => I.InsurenceHolder.Email == User.Identity.Name);
			}


			if (!searchString.IsNullOrEmpty())
			{

				insurences = insurences.Where(I => (I.InsurenceHolder.FirstName + " " + I.InsurenceHolder.LastName).Contains(searchString)
												|| (I.InsurenceHolder.LastName + " " + I.InsurenceHolder.FirstName).Contains(searchString));
			}

			if (!dropdownSearchString.IsNullOrEmpty())
			{
				if (dropdownSearchString == "Vlastní")
				{
					string[] dropdownOptions = { "Pojištění osob", "Pojištění majetku", "Pojištění právní ochrany",
												 "Pojištění odpovědnosti", "Pojištění úvěru nebo záruky", "Pojištění finační ztráty" };


					insurences = insurences.Where(I => !dropdownOptions.Contains(I.TypeOfInsurence));

				}
				else
				{
					insurences = insurences.Where(I => I.TypeOfInsurence.Contains(dropdownSearchString));
				}
			}


			switch (sortOrder)
			{
				case "dateDesc":
					insurences = insurences.OrderByDescending(I => I.ValidUntil);
					break;
				case "dateAsc":
					insurences = insurences.OrderBy(I => I.ValidUntil);
					break;
				case "countDesc":
					insurences = insurences.OrderByDescending(I => I.InsurenceEventsCount);
					break;
				case "countAsc":
					insurences = insurences.OrderBy(I => I.InsurenceEventsCount);
					break;
			}
			int pageSize = 10;
			return View(await PaginatedList<Insurence>.CreateAsync(insurences, pageNumber ?? 1, pageSize));


		}

		// GET: Insurences/Details/5
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Details(int? id, int? insurenceHolderRouteId)
		{
			if (id == null || _context.Insurence == null)
			{
				return NotFound();
			}

			var insurence = await _context.Insurence
				.Include(i => i.InsurenceHolder).Include(E => E.InsurenceEvents)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (insurence == null)
			{
				return NotFound();
			}
			// zajistí aby uživatel nemohl přejít na ostatní položky přes url
			if (User.IsInRole("User") && (insurence.InsurenceHolder.Email != User.Identity.Name))
			{
				TempData["error"] = "Přístup zamítnut";
				return RedirectToAction("Index");
			}
			if (insurenceHolderRouteId != null)
			{
				ViewBag.insurenceHolderRouteId = insurenceHolderRouteId;
			}
			return View(insurence);


		}



		// GET: Insurences/Create
		[Authorize(Roles = "admin")]

		public IActionResult Create(int? insurenceHolderId)
		{
			//Vytvoří instanci třídy aby datum už bylo přednastaveno
			Insurence insurence = new Insurence();
			// Ověří zda byl controller zavolán z pohledu pojistníka
			if (insurenceHolderId != null)
			{
				// Do ViewBagu vloží pojistníka, ke kterému přidáváme pojištění
				ViewBag.Insurer = _context.InsurenceHolder.Where(I => I.Id == insurenceHolderId).First();
			}
			else
			{
				ViewData["InsurenceHolderId"] = new SelectList(_context.InsurenceHolder.Select(I => new
				{
					Id = I.Id,
					DisplayInfo = $"{I.FirstName} : {I.LastName} {I.Email}"
				}), "Id", "DisplayInfo");
			}
			return View(insurence);

		}

		// POST: Insurences/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,TypeOfInsurence,Amount,ValidFrom,ValidUntil,SubjectOfInsurence,InsurenceHolderId")] Insurence insurence, int? insurenceHolderRouteId, string? customText)
		{
			if (ModelState.IsValid)
			{
				if (insurence.TypeOfInsurence == "Vlastní")
				{
					if (customText.IsNullOrEmpty())
					{
						if (insurenceHolderRouteId == null)
						{
							ViewData["InsurenceHolderId"] = new SelectList(_context.InsurenceHolder.Select(I => new
							{
								Id = I.Id,
								DisplayInfo = $"{I.FirstName} : {I.LastName} {I.Email}"
							}), "Id", "DisplayInfo");
						}
						else
						{
							ViewBag.Insurer = _context.InsurenceHolder.Where(I => I.Id == insurenceHolderRouteId).First();
						}
						TempData["error"] = "Pojištění nebylo uvedeno";
						return View(insurence);
					}
					insurence.TypeOfInsurence = customText;
				}
				_context.Add(insurence);
				// Vybere pojistníka, který byl přidán
				var insurenceHolder = _context.InsurenceHolder.Where(i => i.Id == insurence.InsurenceHolderId).First();
				// zvýší pojistníkovi počet jeho pojištění
				insurenceHolder.InsurenceCount++;


				await _context.SaveChangesAsync();

				TempData["success"] = "Pojištění úspěšně vytvořeno";
				// Pokud tvoříme pojištění přes pojistníka, tak vrátí pohled pojistníka
				if (insurenceHolderRouteId != null)
				{
					return RedirectToAction("Details", "InsurenceHolders", new { id = insurence.InsurenceHolderId });
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["InsurenceHolderId"] = new SelectList(_context.InsurenceHolder.Select(I => new
			{
				Id = I.Id,
				DisplayInfo = $"{I.FirstName} : {I.LastName} {I.Email}"
			}), "Id", "DisplayInfo");
			return View(insurence);
		}

		// GET: Insurences/Edit/5
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> Edit(int? id, int? insurenceHolderRouteId)
		{
			if (id == null || _context.Insurence == null)
			{
				return NotFound();
			}

			var insurence = await _context.Insurence.FindAsync(id);
			if (insurence == null)
			{
				return NotFound();
			}
			if (insurenceHolderRouteId != null)
			{
				ViewBag.insurenceHolderRouteId = insurenceHolderRouteId;
			}
			ViewBag.insurenceHolderIdOld = insurence.InsurenceHolderId;
			ViewData["InsurenceHolderId"] = new SelectList(_context.InsurenceHolder.Select(I => new
			{
				Id = I.Id,
				DisplayInfo = $"{I.FirstName} : {I.LastName} {I.Email}"
			}), "Id", "DisplayInfo");
			return View(insurence);
		}

		// POST: Insurences/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,TypeOfInsurence,Amount,ValidFrom,ValidUntil,SubjectOfInsurence,InsurenceHolderId")] Insurence insurence, int? insurenceHolderRouteId, int? insurenceHolderIdOld, string? customText)
		{
			if (id != insurence.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					if (insurence.TypeOfInsurence == "Vlastní")
					{
						if (customText.IsNullOrEmpty())
						{
							if (insurenceHolderRouteId != null)
							{
								ViewBag.insurenceHolderRouteId = insurenceHolderRouteId;
							}
							ViewData["InsurenceHolderId"] = new SelectList(_context.InsurenceHolder.Select(I => new
							{
								Id = I.Id,
								DisplayInfo = $"{I.FirstName} : {I.LastName} {I.Email}"
							}), "Id", "DisplayInfo");
							ViewBag.insurenceHolderIdOld = insurenceHolderIdOld;
							TempData["error"] = "Pojištění nebylo uvedeno";
							return View(insurence);
						}
						insurence.TypeOfInsurence = customText;
					}
					_context.Update(insurence);
					// aktualizuje počet pojištění pro pojištěnce
					if (insurenceHolderIdOld != insurence.InsurenceHolderId)
					{
						var insurenceHolderOld = _context.InsurenceHolder.Where(i => i.Id == insurenceHolderIdOld).First();
						insurenceHolderOld.InsurenceCount--;

						var insurenceHolderNew = _context.InsurenceHolder.Where(i => i.Id == insurence.InsurenceHolderId).First();
						insurenceHolderNew.InsurenceCount++;


					}

					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!InsurenceExists(insurence.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				TempData["success"] = "Změny uloženy";
				// pokud editujeme přes pojistníka, tak vrátí jeho pohled
				if (insurenceHolderRouteId != null)
				{
					return RedirectToAction("Details", "InsurenceHolders", new { id = insurenceHolderRouteId });
				}
				return RedirectToAction(nameof(Index));

			}
			ViewData["InsurenceHolderId"] = new SelectList(_context.InsurenceHolder.Select(I => new
			{
				Id = I.Id,
				DisplayInfo = $"{I.FirstName} : {I.LastName} {I.Email}"
			}), "Id", "DisplayInfo");
			return View(insurence);
		}

		// GET: Insurences/Delete/5
		[Authorize(Roles = "admin, User")]
		public async Task<IActionResult> Delete(int? id, int? insurenceHolderRouteId)
		{
			if (id == null || _context.Insurence == null)
			{
				return NotFound();
			}

			var insurence = await _context.Insurence
				.Include(i => i.InsurenceHolder)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (insurence == null)
			{
				return NotFound();
			}
			if (insurenceHolderRouteId != null)
			{
				ViewBag.insurenceHolderRouteId = insurenceHolderRouteId;
			}
			return View(insurence);
		}

		// POST: Insurences/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id, int? insurenceHolderRouteId)
		{
			if (_context.Insurence == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Insurence'  is null.");
			}
			var insurence = await _context.Insurence.FindAsync(id);
			if (insurence != null)
			{
				// Vymaže pojisté události které patří pojistce
				var events = _context.InsurenceEvent.Where(InsurenceEvent => InsurenceEvent.Id == id).ToList();
				foreach (InsurenceEvent I in events)
				{
					_context.InsurenceEvent.Remove(I);
				}
				// Sníží pojistnikovi počet pojištění
				var insurenceHolder = _context.InsurenceHolder.Where(i => i.Id == insurence.InsurenceHolderId).First();
				insurenceHolder.InsurenceCount--;

				_context.Insurence.Remove(insurence);
			}
			await _context.SaveChangesAsync();
			TempData["warning"] = "Pojištění úspěšné vymazáno";
			// Pokud mažeme pojištění přes pojistníka, tak vrátí pohled pojistníka
			if (insurenceHolderRouteId != null)
			{
				return RedirectToAction("Details", "InsurenceHolders", new { id = insurenceHolderRouteId });
			}

			return RedirectToAction(nameof(Index));
		}
		private bool InsurenceExists(int id)
		{
			return (_context.Insurence?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
