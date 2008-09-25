// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Sql request for scalar result.
  /// </summary>
  public class SqlScalarRequest : SqlRequest
  {
    internal override void CompileWith(SqlDriver driver)
    {
      if (CompilationResult!=null)
        return;
      CompilationResult = driver.Compile(Statement);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    public SqlScalarRequest(ISqlCompileUnit statement)
      : base(statement)
    {
    }
  }
}