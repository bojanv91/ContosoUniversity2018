using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoUniversity.Core
{
    public class RequestValidationException : Exception
    {
        public IEnumerable<ValidationError> Errors { get; private set; } = new List<ValidationError>();

        public RequestValidationException(IEnumerable<ValidationError> errors)
        {
            Errors = errors;
        }
    }
}
