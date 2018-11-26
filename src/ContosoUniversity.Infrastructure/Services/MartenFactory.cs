using System;
using System.Collections.Generic;
using System.Text;
using Marten;

namespace ContosoUniversity.Infrastructure.Services
{
    public static class MartenFactory
    {
        public static IDocumentStore CreateStore(string connectionString)
        {
            return DocumentStore.For(options =>
            {
                options.Connection(connectionString);
                options.Schema.Include<MartenMapping>();
                options.Serializer(new CustomMartenJsonSerializer());
                options.DdlRules.TableCreation = CreationStyle.CreateIfNotExists;
                options.AutoCreateSchemaObjects = AutoCreate.All;   // TODO: Change to NONE.
            });
        }
    }
}
