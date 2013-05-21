// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using Xtensive.Core;
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

    public IEnumerable<PersistParameterBinding> ParameterBindings { get; private set; }

    public SqlCompilationResult GetCompiledStatement()
    {
      if (compiledStatement==null)
        throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);
      return compiledStatement;
    }

    public void Prepare()
    {
      if (compiledStatement!=null)
        return;
      compiledStatement = driver.Compile(CompileUnit);
      CompileUnit = null;
      Statement = null;
    }

    // Constructors

    public PersistRequest(
      StorageDriver driver, SqlStatement statement, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");

      var compileUnit = statement as ISqlCompileUnit;
      if (compileUnit==null)
        throw new ArgumentException("Statement is not ISqlCompileUnit");

      this.driver = driver;
      Statement = statement;
      CompileUnit = compileUnit;
      ParameterBindings = ParameterBinding.NormalizeBindings(parameterBindings);
    }
  }
}