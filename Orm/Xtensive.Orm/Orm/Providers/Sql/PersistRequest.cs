// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Modification (INSERT, UPDATE, DELETE) request.
  /// </summary>
  public class PersistRequest
  {
    private ISqlCompileUnit compileUnit;
    private SqlCompilationResult compiledStatement;

    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<PersistParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets statement.
    /// </summary>
    public SqlStatement Statement { get; private set; }

    /// <summary>
    /// Gets compiled statement.
    /// </summary>
    public SqlCompilationResult GetCompiledStatement()
    {
      if (compiledStatement==null)
        throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);
      return compiledStatement;
    }

    /// <inheritdoc/>
    public void Prepare(DomainHandler domainHandler)
    {
      if (compiledStatement!=null)
        return;
      compiledStatement = domainHandler.Driver.Compile(compileUnit);
      compileUnit = null;
      Statement = null;
    }

    private void Initialize(SqlStatement statement, ISqlCompileUnit unit, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      Statement = statement;
      compileUnit = unit;
      ParameterBindings = parameterBindings!=null
        ? parameterBindings.ToHashSet()
        : new HashSet<PersistParameterBinding>();
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public PersistRequest(SqlInsert statement, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      Initialize(statement, statement, parameterBindings);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public PersistRequest(SqlUpdate statement, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      Initialize(statement, statement, parameterBindings);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public PersistRequest(SqlDelete statement, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      Initialize(statement, statement, parameterBindings);
    }


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public PersistRequest(SqlBatch statement, IEnumerable<PersistParameterBinding> parameterBindings)
    {
      Initialize(statement, statement, parameterBindings);
    }
  }
}