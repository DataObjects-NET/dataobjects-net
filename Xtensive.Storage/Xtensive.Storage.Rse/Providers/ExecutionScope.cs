// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.16

using Xtensive.Core;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// The scope for <see cref="ExecutionContext"/>.
  /// </summary>
  public sealed class ExecutionScope: Scope<ExecutionContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public static new ExecutionContext CurrentContext {
      get {
        return Scope<ExecutionContext>.CurrentContext;
      }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new ExecutionContext Context {
      get {
        return base.Context;
      }
    }


    // Constructors

    /// <inheritdoc/>
    public ExecutionScope(ExecutionContext context)
      : base(context)
    {
    }
  }
}