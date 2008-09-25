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
    protected SqlCompilerResults CompilationResult;

    /// <summary>
    /// Gets or sets the statement.
    /// </summary>
    public ISqlCompileUnit Statement { get; private set; }

    /// <summary>
    /// Gets the compiled statement.
    /// </summary>
    public string CompiledStatement
    {
      get { return CompilationResult.CommandText; }
    }

    internal abstract void CompileWith(SqlDriver driver);


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