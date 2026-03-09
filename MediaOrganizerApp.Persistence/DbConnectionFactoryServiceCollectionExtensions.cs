using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Persistence
{
    public static class DbConnectionFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddDbConnectionFactory(this IServiceCollection collection, Dictionary<string, string> connections)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.AddScoped<IDbConnectionFactory, DbConnectionFactory>(factory => new DbConnectionFactory(connections));
        }
    }
}
