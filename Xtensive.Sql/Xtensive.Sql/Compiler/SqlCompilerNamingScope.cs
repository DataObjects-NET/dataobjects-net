// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;

namespace Xtensive.Sql.Compiler
{
  [Serializable]
  public class SqlCompilerNamingScope : IDisposable
  {
    private readonly SqlCompilerContext context;

    internal SqlCompilerNamingOptions ParentOptions { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
      context.CloseScope(this);
    }

    public SqlCompilerNamingScope(SqlCompilerContext context, SqlCompilerNamingOptions parentOptions)
    {
      this.context = context;
      ParentOptions = parentOptions;
    }
  }
}