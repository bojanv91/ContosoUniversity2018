using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContosoUniversity.Core
{
    /// <summary>
    /// Represents a general purpose exception for all domain concerns. Usually used to represent
    /// bussiness rule violation. Recommended is to inherit this class to define a business rule violation.
    /// </summary>
    public class CoreException : Exception
    {
        public IEnumerable<ValidationError> Errors { get; private set; }

        public CoreException(string message) : base(message)
        {
            Errors = new List<ValidationError>()
            {
                new ValidationError { PropertyName = "CoreException", ErrorMessage = message }
            };
        }

        public CoreException(IEnumerable<string> messages) : base(messages.Count() + " errors occured.")
        {
            Errors = messages.Select(message => new ValidationError
            {
                PropertyName = "CoreException",
                ErrorMessage = message
            });
        }

        public CoreException(IEnumerable<ValidationError> messages) : base(messages.Count() + " errors occured.")
        {
            Errors = messages;
        }
    }   
}
