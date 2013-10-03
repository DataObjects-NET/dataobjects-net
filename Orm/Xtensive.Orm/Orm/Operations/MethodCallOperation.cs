// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes arbitrary method call operation.
  /// </summary>
  [Serializable]
  public sealed class MethodCallOperation : Operation
  {
    private readonly Action<OperationExecutionContext, object[]> prepareAction;
    private readonly Action<OperationExecutionContext, object[]> executeAction;
    private readonly object[] arguments;

    /// <summary>
    /// Gets the <see cref="Operation.Prepare"/> method action.
    /// </summary>
    public Action<OperationExecutionContext, object[]> PrepareAction
    {
      get { return prepareAction; }
    }

    /// <summary>
    /// Gets the <see cref="Operation.Execute"/> method action.
    /// </summary>
    public Action<OperationExecutionContext, object[]> ExecuteAction
    {
      get { return executeAction; }
    }

    /// <summary>
    /// Gets the arguments.
    /// </summary>
    public object[] Arguments
    {
      get { return arguments; }
    }

    /// <inheritdoc/>
    public override string Title {
      get { return "Method call"; }
    }

    /// <inheritdoc/>
    public override string Description {
      get {
        return string.Format("{0}: {1}({2})",
          Title, executeAction.Method.GetShortName(true), arguments.ToCommaDelimitedString());
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      if (prepareAction!=null)
        prepareAction.Invoke(context, arguments);
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      executeAction.Invoke(context, arguments);
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new MethodCallOperation(prepareAction, executeAction, arguments);
      return clone;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="executeAction">The <see cref="Operation.Execute"/> method action.</param>
    /// <param name="arguments">The action arguments.</param>
    public MethodCallOperation(
      Action<OperationExecutionContext, object[]> executeAction, 
      params object[] arguments)
      : this(null, executeAction, arguments)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="prepareAction">The <see cref="Operation.Prepare"/> method action.</param>
    /// <param name="executeAction">The <see cref="Operation.Execute"/> method action.</param>
    /// <param name="arguments">The action arguments.</param>
    public MethodCallOperation(
      Action<OperationExecutionContext, object[]> prepareAction, 
      Action<OperationExecutionContext, object[]> executeAction,
      params object[] arguments)
    {
      ArgumentValidator.EnsureArgumentNotNull(executeAction, "executeAction");
      this.prepareAction = prepareAction;
      this.executeAction = executeAction;
      this.arguments = arguments;
    }
  }
}