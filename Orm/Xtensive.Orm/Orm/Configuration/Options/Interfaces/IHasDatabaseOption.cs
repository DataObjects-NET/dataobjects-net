// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Xtensive.Orm.Configuration.Options
{
  internal interface IHasDatabaseOption : IIdentifyableOptions
  {
    /// <summary>
    /// Database (real or alias). In cases when database is part of instance identifier
    /// it might require to be mapped
    /// </summary>
    string Database { get; set; }

    /// <summary>
    /// Tries to map database part of identifier from alias to real database names.
    /// It might be required if <see cref="DomainConfiguration.Databases"/> declare
    /// aliases and 
    /// </summary>
    /// <param name="databaseMap">Map of databases</param>
    /// <returns>If alias is used then new identifier with substituted alias, otherwise, the same identifier.</returns>
    object GetMappedIdentifier(IDictionary<string, DatabaseOptions> databaseMap);
  }
}
