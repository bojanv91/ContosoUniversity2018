using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core
{
    public interface IRootEntity : IEntity
    {
        DateTime CreatedOnUtc { get; set; }
        DateTime LastModifiedOnUtc { get; set; }
    }
}
