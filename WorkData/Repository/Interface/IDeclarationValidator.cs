using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkData.Models;

namespace WorkData.Repository.Interface
{
    // Interfaccia del servizio di validazione
    public interface IDeclarationValidator
    {
        void Validate(Declaration declaration, ModelStateDictionary modelState, bool? declarationExist);
    }
}
