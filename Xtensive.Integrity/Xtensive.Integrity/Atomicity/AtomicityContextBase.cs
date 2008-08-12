// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Integrity.Atomicity.OperationLogs;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// The context describing current atomic operation.
  /// </summary>
  public abstract class AtomicityContextBase: Context<AtomicityScope>
  {
    private IOperationDescriptorFactory operationDescriptorFactory;
    private IOperationLog operationLog;
    private AtomicityContextOptions options;

    [DebuggerStepThrough]
    public AtomicityContextOptions Options
    {
      get { return options; }
      protected set { options = value; }
    }

    [DebuggerStepThrough]
    public IOperationDescriptorFactory OperationDescriptorFactory
    {
      get { return operationDescriptorFactory; }
    }

    public IOperationLog OperationLog {
      [DebuggerStepThrough]
      get { return operationLog; }
      set {
        if (operationLog != null)
          throw Exceptions.AlreadyInitialized("OperationLog");
        operationLog = value;
      }
    }

    public override bool IsActive
    {
      get { return AtomicityScope.CurrentContext == this; }
    }

    #region Events (protected virtual handlers)

    protected internal virtual void OnActivate(AtomicityScope scope)
    {
    }

    protected internal virtual void OnActivateRedoScope(RedoScope redoScope)
    {
    }

    protected internal virtual void OnActivateUndoScope(UndoScope undoScope)
    {
    }

    protected internal virtual void OnDeactivate(AtomicityScope scope)
    {
    }

    protected internal virtual void OnDeactivateRedoScope(RedoScope redoScope)
    {
    }

    protected internal virtual void OnDeactivateUndoScope(UndoScope undoScope)
    {
    }

    #endregion

    protected virtual void Initialize(AtomicityContextOptions options, IOperationLog operationLog)
    {
      this.options = options;
      this.operationLog = operationLog;
      operationDescriptorFactory = new OperationDescriptorFactory();
    }

    /// <inheritdoc/>
    protected override AtomicityScope CreateActiveScope()
    {
      return new AtomicityScope(this);
    }


    // Constructors

    public AtomicityContextBase()
      : this(AtomicityContextOptions.Default)
    {
    }

    public AtomicityContextBase(IOperationLog operationLog)
      : this(AtomicityContextOptions.Default, operationLog)
    {
    }

    public AtomicityContextBase(AtomicityContextOptions options)
      : this(options, null)
    {
    }

    public AtomicityContextBase(AtomicityContextOptions options, IOperationLog operationLog)
    {
      Initialize(options, operationLog);
    }
  }
}