// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// <see cref="OperationContext"/> realted extensions.
  /// </summary>
  public static class OperationContextExtensions
  {
    /// <summary>
    /// Determines whether the context is enabled.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>
    /// 	<see langword="true"/> if the specified context is enabled; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsEnabled(this OperationContext context)
    {
      return context != null && context != OperationContext.Default && !context.completed;
    }

    /// <summary>
    /// Completes the operation context.
    /// </summary>
    /// <param name="context">The context.</param>
    public static void Complete(this OperationContext context)
    {
      if (context != null)
        context.completed = true;
    }

    /// <summary>
    /// Adds the specified operation to the context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="operation">The operation.</param>
    public static void Add(this OperationContext context, IOperation operation)
    {
      if (context != null)
        context.Operations.Add(operation);
    }
  }
}