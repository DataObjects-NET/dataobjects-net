// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using System.Collections.Generic;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Compiler;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SqlRequest
  {
    private SqlCompilerResults compilationResult;

    public ISqlCompileUnit Statement { get; private set; }

    public string CompiledStatement
    {
      get { return compilationResult.CommandText; }
    }

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

    protected SqlRequest(ISqlCompileUnit statement)
    {
      Statement = statement;
    }
  }
}