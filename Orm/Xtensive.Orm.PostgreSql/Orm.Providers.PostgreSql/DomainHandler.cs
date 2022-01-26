// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.PostgreSql
{
  /// <summary>
  /// A domain handler specific to PostgreSQL RDBMS.
  /// </summary>
  public class DomainHandler : Providers.DomainHandler
  {
    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration) =>
      new SqlCompiler(Handlers, configuration);

    /// <inheritdoc/>
    protected override IEnumerable<Type> GetProviderCompilerContainers()
    {
      return base.GetProviderCompilerContainers()
        .Concat(new[] {
          typeof(NpgsqlPointCompilers),
          typeof(NpgsqlLSegCompilers),
          typeof(NpgsqlBoxCompilers),
          typeof(NpgsqlCircleCompilers),
          typeof(NpgsqlPathCompilers),
          typeof(NpgsqlPolygonCompilers)
        });
    }
  }
}