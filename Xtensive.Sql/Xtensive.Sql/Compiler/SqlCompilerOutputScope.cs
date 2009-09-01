// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Compiler.Internals;

namespace Xtensive.Sql.Compiler
{
  public class SqlCompilerOutputScope : IDisposable
  {
    private readonly SqlCompilerContext context;

    internal ContextType Type { get; private set; }

    internal NodeContainer ParentContainer { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
      context.CloseScope(this);
    }

    internal SqlCompilerOutputScope(SqlCompilerContext context, NodeContainer parentContainer, ContextType type)
    {
      this.context = context;
      Type = type;
      ParentContainer = parentContainer;
    }
  }
}