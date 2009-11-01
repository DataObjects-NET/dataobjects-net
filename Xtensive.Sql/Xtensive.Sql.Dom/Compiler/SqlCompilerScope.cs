// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Compiler.Internals;

namespace Xtensive.Sql.Dom.Compiler
{
  public class SqlCompilerScope : IDisposable
  {
    private SqlCompilerContext context;
    private NodeContainer output;
    private ContextType type;

    internal ContextType Type
    {
      get { return type; }
    }

    internal NodeContainer Output
    {
      get { return output; }
    }

    public void Dispose()
    {
      context.DisposeScope(this);
      context = null;
      output = null;
    }

    internal SqlCompilerScope(SqlCompilerContext context, NodeContainer output, ContextType type)
    {
      this.context = context;
      this.type = type;
      this.output = output;
    }
  }
}