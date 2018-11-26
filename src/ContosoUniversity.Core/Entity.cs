using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core
{
    /// <summary>
    /// Entity by Domain Driven Design terms.
    /// </summary>
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

}
