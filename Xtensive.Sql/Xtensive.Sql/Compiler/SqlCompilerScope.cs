// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Compiler.Internals;

namespace Xtensive.Sql.Compiler
{
  public class SqlCompilerScope : IDisposable
  {
    private readonly SqlCompilerContext context;
    private readonly NodeContainer originalOutput;
    private readonly ContextType type;

    internal ContextType Type { get { return type; } }
    internal NodeContainer OriginalOutput { get { return originalOutput; } }

    public void Dispose()
    {
      context.DisposeScope(this);
    }

    internal SqlCompilerScope(SqlCompilerContext context, NodeContainer originalOutput, ContextType type)
    {
      this.context = context;
      this.type = type;
      this.originalOutput = originalOutput;
    }
  }
}