using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;

namespace Marten
{
    public static class MartenExtensions
    {
        /// <summary>
        /// Loads document, or throws CoreException if no document is found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<T> LoadStrictAsync<T>(this IDocumentSession session, Guid? id, CancellationToken token = default(CancellationToken)) where T : class
        {
            if (!id.HasValue)
                throw new CoreException($"{typeof(T).Name} is not found.");

            return session.LoadAsync<T>(id.Value, token)
                ?? throw new CoreException($"{typeof(T).Name} is not found.");
        }
    }
}
