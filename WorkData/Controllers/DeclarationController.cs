using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkData.Data;
using WorkData.Data.GenericModels;
using WorkData.Models;
using WorkData.Repository.Interface;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace WorkData.Controllers
{
    [Authorize]
    public class DeclarationController : Controller
    {
        #region Vars
        private readonly AppDbContext _db;
        private readonly IDeclarationValidator _validator;
        private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Today);

        #endregion

        #region Constructor
        public DeclarationController(AppDbContext db, IDeclarationValidator validator)
        {
            _db = db;
            _validator = validator;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            var declarations = _db.Declarations.ToList();
            var groupedByMonthAndYear = declarations
                .GroupBy(d => new MonthYearGroup(d.Date.Year, d.Date.Month))
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .ToList();

            List<Weeks> weeks = LoadWeeks();
            ViewBag.Weeks = weeks;

            return View(groupedByMonthAndYear);

        }
        #endregion

        #region Create/FastCreate/WeekCreate
        //[HttpGet]
        //public IActionResult WeekCreate()


        [HttpPost]
        public IActionResult WeekCreate([FromBody] Weeks week)
        {
            // L'oggetto 'week' è valido.
            // L'obiettivo è validare e salvare le dichiarazioni per ogni giorno della settimana lavorativa.

            var daysToProcess = new List<Declaration>();
            var errors = new List<string>();

            DateTime currentDate = week.FirstWeekDay;

            // Cicla attraverso ogni giorno, dall'inizio alla fine della settimana
            while (currentDate <= week.LastWeekDay)
            {
                // Se il giorno è sabato o domenica, lo salta e passa al prossimo
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    currentDate = currentDate.AddDays(1);
                    continue; // Passa all'iterazione successiva del ciclo
                }

                Declaration d = new Declaration()
                {
                    Date = DateOnly.FromDateTime(currentDate),
                    OrdinalHours = 8,
                };

                Declaration? alreadyExist = CheckIfAlreadyExist(d);
                if (alreadyExist != null)
                {
                    _validator.Validate(alreadyExist, ModelState, true);
                }
                else
                {
                    _validator.Validate(d, ModelState, false);
                }

                // Raccoglie tutti gli errori in una lista
                errors.AddRange(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                // Pulisce il ModelState per l'iterazione successiva
                ModelState.Clear();

                // Aggiunge la dichiarazione alla lista da salvare
                daysToProcess.Add(d);

                // Passa al giorno successivo
                currentDate = currentDate.AddDays(1);
            }

            // Se ci sono errori, non salvare nulla e restituisci un messaggio
            if (errors.Any())
            {
                var uniqueErrors = errors.Distinct().ToList();

                if (uniqueErrors.Any())
                {
                    TempData["error"] = string.Join("\n\r", uniqueErrors);
                }
                return Json(new { success = false, message = TempData["error"] });
            }

            // Se la validazione è andata a buon fine, salvi tutti i dati in un'unica operazione
            _db.Declarations.AddRange(daysToProcess);
            _db.SaveChanges();

            TempData["success"] = "Week added successfully";
            return Json(new { success = true, message = "Settimana creata con successo!" });

        }

        [HttpPost]
        public IActionResult FastCreate()
        {
            Declaration d = new Declaration()
            {
                Date = _today,
                OrdinalHours = 8,
            };
            Declaration? alreadyExist = CheckIfAlreadyExist(d);
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
                Date = _today,
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
            Declaration? alreadyExist = CheckIfAlreadyExist(d);
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
        #endregion

        #region Edit
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
            Declaration? alreadyExist = CheckIfAlreadyExist(d);
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
                _db.Declarations.Update(d);
                _db.SaveChanges();
                TempData["success"] = "Declaration updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        #endregion

        #region Delete
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
        #endregion

        #region Check if declaration already exist
        private Declaration? CheckIfAlreadyExist(Declaration d)
        {
            Declaration? alreadyExist = _db.Declarations.FirstOrDefault(x => x.Date == d.Date);
            return alreadyExist;
        }
        #endregion

        #region Load weeks 
        private List<Weeks> LoadWeeks()
        {
            List<Weeks> weeks = new List<Weeks>();
            var currentYear = DateTime.Now.Year;
            var firstDayOfYear = new DateTime(currentYear, 1, 1);
            var firstWeekStart = firstDayOfYear.AddDays(-(int)firstDayOfYear.DayOfWeek + (int)DayOfWeek.Monday);

            for (int i = 0; i < 52; i++)
            {
                Weeks w = new Weeks()
                {
                    WeekNumber = i + 1,
                    FirstWeekDay = firstWeekStart.AddDays(i * 7),
                    LastWeekDay = firstWeekStart.AddDays(i * 7 + 6)
                };

                weeks.Add(w);
            }
            return weeks;

        }
        #endregion

    }
}
