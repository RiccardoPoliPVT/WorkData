using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkData.Data;
using WorkData.Data.GenericModels;
using WorkData.Models;
using WorkData.Repository.Interface;

namespace WorkData.Controllers
{
    public class DeclarationController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IDeclarationValidator _validator;
        public DeclarationController(AppDbContext db, IDeclarationValidator validator)
        {
            _db = db;
            _validator = validator;
        }

        public IActionResult Index()
        {
            var declarations = _db.Declarations.ToList();
            var groupedByMonthAndYear = declarations
                .GroupBy(d => new MonthYearGroup(d.Date.Year, d.Date.Month))
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .ToList();


            return View(groupedByMonthAndYear);

        }

        [HttpPost]
        public IActionResult FastCreate()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today.Date);
            Declaration d = new Declaration()
            {
                Date = today,
                OrdinalHours = 8,
            };
            Declaration? alreadyExist = _db.Declarations.FirstOrDefault(x => x.Date == today);
            if (alreadyExist != null)
            {
                _validator.Validate(alreadyExist, ModelState, true);

            }
            else
            {
                _validator.Validate(d, ModelState, false);

            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
            if (errors.Any())
            {
                TempData["error"] = string.Join("\n", errors);
            }

            if (ModelState.IsValid)
            {
                _db.Declarations.Add(d);
                _db.SaveChanges();
                TempData["success"] = "Declaration added successfully";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            Declaration d = new Declaration()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.Date),
                OrdinalHours = 8,
                ExtraHours = 0,
                SickHours = 0,
                HolidayHours = 0,
                PermissionHours = 0
            };
            return View(d);
        }

        [HttpPost]
        public IActionResult Create(Declaration d)
        {
            Declaration? alreadyExist = _db.Declarations.FirstOrDefault(x => x.Date == d.Date);
            if (alreadyExist != null)
            {
                _validator.Validate(alreadyExist, ModelState, true);

            }
            else
            {
                _validator.Validate(d, ModelState, false);

            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
            if (errors.Any())
            {
                TempData["error"] = string.Join("\n", errors);
            }


            if (ModelState.IsValid)
            {
                _db.Declarations.Add(d);
                _db.SaveChanges();
                TempData["success"] = "Declaration added successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Declaration? d = _db.Declarations.Find(id);
            if (d == null)
            {
                return NotFound();
            }
            return View(d);
        }
        [HttpPost]
        public IActionResult Edit(Declaration d)
        {
            if (d.ExtraHours == 0 && d.OrdinalHours == 0)
            {
                ModelState.AddModelError("OrdinalHours", "Ordinal hours must be more then 1");
                ModelState.AddModelError("ExtraHours", "Extra hours must be more then 1");

            }
            if (d.Date > DateOnly.FromDateTime(DateTime.Today.Date))
            {
                ModelState.AddModelError("Date", "Cannot insert a date higher then today");
            }
            //Se le ore ordinarie più malattia+ferie+permessi superano le 8 ore -> Errore
            float regularHours = d.OrdinalHours + d.SickHours + d.HolidayHours + d.PermissionHours;
            if (regularHours > 8)
            {
                ModelState.AddModelError("", "Cannot insert more then 8 hours as 'Regular' hours");
                TempData["error"] = "Cannot insert more then 8 hours as 'Regular' hours (Ordinal/Sick/Holiday/Permission)";

            }
            if (ModelState.IsValid)
            {
                _db.Declarations.Update(d);
                _db.SaveChanges();
                TempData["success"] = "Declaration updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }


        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Declaration? d = _db.Declarations.Find(id);
            if (d == null)
            {
                return NotFound();
            }
            _db.Declarations.Remove(d);
            _db.SaveChanges();
            TempData["success"] = "Declaration deleted successfully";
            return RedirectToAction("Index");

        }

    }
}
