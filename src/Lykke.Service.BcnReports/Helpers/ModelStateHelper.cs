using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web.Helpers
{
    public static class ModelStateHelper
    {
        public static List<string> GetErrorsList(this ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                from error in state.Errors
                select error.ErrorMessage;

            var errorList = query.ToList();
            return errorList;
        }
    }
}
