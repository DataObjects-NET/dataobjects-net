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
  /// <see cref="IOperation"/> implementation that describes method call.
  /// </summary>
  [Serializable]
  public sealed class MethodCallOperation : IOperation
  {
    /// <inheritdoc/>
    public void Prepare(OperationExecutionContext context)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Execute(OperationExecutionContext context)
    {
      throw new NotImplementedException();
    }
  }
}