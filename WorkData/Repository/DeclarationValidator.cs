using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkData.Models;
using WorkData.Repository.Interface;

namespace WorkData.Repository
{
    // Implementazione del servizio di validazione
    public class DeclarationValidator : IDeclarationValidator
    {
        public void Validate(Declaration d, ModelStateDictionary modelState,bool? declarationExist = null)
        {
            // Tutte le tue regole di validazione vanno qui, usando modelState per aggiungere errori
            if (d.OrdinalHours + d.ExtraHours + d.SickHours + d.HolidayHours + d.PermissionHours <= 0)
            {
                modelState.AddModelError("", "You must insert at least 1 hour in a day.");
            }

            if (d.Date > DateOnly.FromDateTime(DateTime.Today.Date))
            {
                modelState.AddModelError("Date", "Cannot insert a future date.");
            }

            //Significa che devo controllare se la timbratura è già stata inserita
            if (declarationExist != null && declarationExist == true)
            {
                //TempData["error"] = "Cannot insert date: " + d.Date + " cause it already exists.\n\r Update the existing one ";
                modelState.AddModelError("Date", "Cannot insert date: " + d.Date + " cause it already exists.\n\r Update the existing one ");
            }

            // Non si possono inserire 0 ore in tutto 
            if (d.ExtraHours == 0 && d.OrdinalHours == 0 && d.SickHours == 0 && d.HolidayHours == 0 && d.PermissionHours == 0)
            {
                modelState.AddModelError("", "Ordinal hours must be more then 1");
            }

            //Le ore ordinarie non devono mai essere > 8
            float regularHours = d.OrdinalHours + d.SickHours + d.HolidayHours + d.PermissionHours;
            if (regularHours > 8)
            {
                modelState.AddModelError("", "Cannot insert more then 8 hours as 'Regular' hours (Ordinal/Sick/Holiday/Permission)");
            }

            //Il totale delle ore giornaliere non può essere > 24
            if(regularHours+d.ExtraHours > 24)
            {
                modelState.AddModelError("","Total hours of the day cannot be over 24 hours");
            }
        }
    }
}
