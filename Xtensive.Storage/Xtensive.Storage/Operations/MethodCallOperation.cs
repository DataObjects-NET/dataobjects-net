// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// <see cref="IOperation"/> implementation that describes method call.
  /// </summary>
  [Serializable]
  public sealed class MethodCallOperation : IOperation
  {
    private readonly Action<OperationExecutionContext> prepare;
    private readonly Action<OperationExecutionContext> execute;

    /// <inheritdoc/>
    public void Prepare(OperationExecutionContext context)
    {
      prepare(context);
    }

    /// <inheritdoc/>
    public void Execute(OperationExecutionContext context)
    {
      execute(context);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="prepare">The <see cref="Prepare"/> method action.</param>
    /// <param name="execute">The <see cref="Execute"/> method action.</param>
    public MethodCallOperation(Action<OperationExecutionContext> prepare, Action<OperationExecutionContext> execute)
    {
      this.prepare = prepare;
      this.execute = execute;
    }
  }
}