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

    internal NodeConfiguration NodeConfiguration { get; }

    public SqlCompilationResult GetCompiledStatement() =>
      compiledStatement ?? throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);

    public void Prepare()
    {
      if (compiledStatement != null)
        return;
      compiledStatement = (NodeConfiguration != null)
        ? driver.Compile(CompileUnit, NodeConfiguration)
        : driver.Compile(CompileUnit);
      CompileUnit = null;
      Statement = null;
    }

    // Constructors

    public PersistRequest(
      StorageDriver driver, SqlStatement statement, IEnumerable<PersistParameterBinding> parameterBindings)
      : this(driver, statement, parameterBindings, null)
    {
    }

    public PersistRequest(
      StorageDriver driver, SqlStatement statement, IEnumerable<PersistParameterBinding> parameterBindings, NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");

      var compileUnit = statement as ISqlCompileUnit
        ?? throw new ArgumentException("Statement is not ISqlCompileUnit");

      this.driver = driver;
      Statement = statement;
      CompileUnit = compileUnit;
      ParameterBindings = ParameterBinding.NormalizeBindings(parameterBindings);
      NodeConfiguration = nodeConfiguration;
    }
  }
}
