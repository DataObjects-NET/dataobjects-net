// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Modification (INSERT, UPDATE, DELETE) request.
  /// </summary>
  public sealed class PersistRequest
  {
    private readonly StorageDriver driver;

    private SqlCompilationResult compiledStatement;

    public SqlStatement Statement { get; private set; }

    public ISqlCompileUnit CompileUnit { get; private set; }

    public IReadOnlyCollection<PersistParameterBinding> ParameterBindings { get; }

    public SqlCompilationResult GetCompiledStatement() =>
      compiledStatement ?? throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);

    public void Prepare()
    {
      if (compiledStatement != null)
        return;
      compiledStatement = driver.Compile(CompileUnit);
      CompileUnit = null;
      Statement = null;
    }

    // Constructors

    public PersistRequest(
      StorageDriver driver, SqlStatement statement, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
      Statement = statement ?? throw new ArgumentNullException(nameof(statement));
      CompileUnit = statement as ISqlCompileUnit ?? throw new ArgumentException("Statement is not ISqlCompileUnit");
      ParameterBindings = ParameterBinding.NormalizeBindings(parameterBindings);
    }
  }
}
