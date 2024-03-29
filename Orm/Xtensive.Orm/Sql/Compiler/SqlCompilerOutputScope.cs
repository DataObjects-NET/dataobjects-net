// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// SQL compiler output scope.
  /// </summary>
  public readonly struct SqlCompilerOutputScope : IDisposable
  {
    private readonly SqlCompilerContext context;

    internal ContextType Type { get; }

    internal ContainerNode ParentContainer { get; }
    
    internal bool StartOfCollection { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
      context.CloseScope(this);
    }

    internal SqlCompilerOutputScope(SqlCompilerContext context, ContextType type)
    {
      this.context = context;
      Type = type;
      ParentContainer = context.Output;
      StartOfCollection = ParentContainer.StartOfCollection;
    }
  }
}
