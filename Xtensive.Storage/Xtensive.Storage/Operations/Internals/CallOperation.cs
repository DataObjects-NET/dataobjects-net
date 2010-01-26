// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.26

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation invoking a delegate.
  /// </summary>
  public class CallOperation : Operation
  {
    /// <summary>
    /// The delegate to be invoked.
    /// </summary>
    public readonly Action Action;

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {}

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      Action.Invoke();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CallOperation(Action action)
      : base(OperationType.MethodCall)
    {
      ArgumentValidator.EnsureArgumentNotNull(action, "action");

      Action = action;
    }
  }
}