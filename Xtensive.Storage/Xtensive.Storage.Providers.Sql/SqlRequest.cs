// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Compiler;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Base class for any sql request.
  /// </summary>
  public abstract class SqlRequest
  {
    private SqlCompilerResults compilationResult;

    /// <summary>
    /// Gets or sets the statement.
    /// </summary>
    public ISqlCompileUnit Statement { get; private set; }

    /// <summary>
    /// Gets the compiled statement.
    /// </summary>
    public string CompiledStatement
    {
      get { return compilationResult.CommandText; }
    }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    /// <returns>The <see cref="List{T}"/> of <see cref="SqlParameter"/> instances.</returns>
    public abstract List<SqlParameter> GetParameters();

    internal virtual  void CompileWith(SqlDriver driver)
    {
      if (compilationResult!=null)
        return;
      int i = 0;
      List<SqlParameter> parameters = GetParameters();
      if (parameters != null)
        foreach (SqlParameter p in GetParameters())
          p.ParameterName = "p" + i++;
      compilationResult = driver.Compile(Statement);
    }


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