// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Compiler;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Base class for any SQL request.
  /// </summary>
  public abstract class SqlRequest
  {
    protected internal SqlCompilationResult CompilationResult;

    /// <summary>
    /// Gets or sets the statement.
    /// </summary>
    public ISqlCompileUnit Statement { get; private set; }

    /// <summary>
    /// Gets the compiled statement.
    /// </summary>
    public string CompiledStatement
    {
      get { return CompilationResult.GetCommandText(); }
    }

    internal virtual void Compile(DomainHandler domainHandler)
    {
      if (CompilationResult!=null)
        return;

      CompileParameters(domainHandler);
      CompileStatement(domainHandler);
    }

    /// <summary>
    /// Compiles the parameters.
    /// </summary>
    /// <param name="domainHandler">The domain handler.</param>
    protected void CompileParameters(DomainHandler domainHandler)
    {
      var bindings = GetParameterBindings();
      if (bindings == null)
        return;
      int i = 0;
      foreach (var binding in bindings) {
        binding.SqlParameter.ParameterName = "p" + i++;
        if (binding.TypeMapping == null)
          continue;
        binding.SqlParameter.DbType = binding.TypeMapping.DbType;
      }
    }

    /// <summary>
    /// Compiles the <see cref="Statement"/>.
    /// </summary>
    /// <param name="domainHandler">The domain handler.</param>
    protected void CompileStatement(DomainHandler domainHandler)
    {
      CompilationResult = domainHandler.SqlDriver.Compile(Statement);
    }

    protected virtual void BindParameter(SqlParameterBinding binding, object value)
    {
      if (value != null && value != DBNull.Value && binding.TypeMapping != null && binding.TypeMapping.ToSqlValue != null)
        value = binding.TypeMapping.ToSqlValue(value);
      binding.SqlParameter.Value = value;
    }

    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    protected abstract IEnumerable<SqlParameterBinding> GetParameterBindings();


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    protected SqlRequest(ISqlCompileUnit statement)
    {
      Statement = statement;
    }
  }
}