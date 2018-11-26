using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core
{
    /// <summary>
    /// Aggregate root in Domain Driven Design terms.
    /// </summary>
    public abstract class RootEntity : Entity, IRootEntity
    {
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedOnUtc { get; set; } = DateTime.UtcNow;
    }
}
