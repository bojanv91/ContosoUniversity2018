using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
