// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

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

    /// <summary>
    /// Gets or sets the atomicity context options.
    /// </summary>
    public AtomicityContextOptions Options {
      [DebuggerStepThrough]
      get { return options; }
      [DebuggerStepThrough]
      protected set { options = value; }
    }

    /// <summary>
    /// Gets the operation descriptor factory.
    /// </summary>
    public IOperationDescriptorFactory OperationDescriptorFactory {
      [DebuggerStepThrough]
      get { return operationDescriptorFactory; }
    }

    /// <summary>
    /// Gets or sets the operation log.
    /// </summary>
    public IOperationLog OperationLog {
      [DebuggerStepThrough]
      get { return operationLog; }
      set {
        if (operationLog != null)
          throw Exceptions.AlreadyInitialized("OperationLog");
        operationLog = value;
      }
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return AtomicityScope.CurrentContext == this; }
    }

    #region Events (protected virtual handlers)

    /// <summary>
    /// Called when context is being activated.
    /// </summary>
    /// <param name="scope">The scope.</param>
    protected internal virtual void OnActivate(AtomicityScope scope)
    {
    }

    /// <summary>
    /// Called when redo scope is being activated.
    /// </summary>
    /// <param name="redoScope">The redo scope.</param>
    protected internal virtual void OnActivateRedoScope(RedoScope redoScope)
    {
    }

    /// <summary>
    /// Called when undo scope is being activated.
    /// </summary>
    /// <param name="undoScope">The undo scope.</param>
    protected internal virtual void OnActivateUndoScope(UndoScope undoScope)
    {
    }

    /// <summary>
    /// Called when context is being deactivated.
    /// </summary>
    /// <param name="scope">The scope.</param>
    protected internal virtual void OnDeactivate(AtomicityScope scope)
    {
    }

    /// <summary>
    /// Called when redo scope is being deactivated.
    /// </summary>
    /// <param name="redoScope">The redo scope.</param>
    protected internal virtual void OnDeactivateRedoScope(RedoScope redoScope)
    {
    }

    /// <summary>
    /// Called when undo scope is being deactivated.
    /// </summary>
    /// <param name="undoScope">The undo scope.</param>
    protected internal virtual void OnDeactivateUndoScope(UndoScope undoScope)
    {
    }

    #endregion

    /// <summary>
    /// Initializes the context.
    /// </summary>
    /// <param name="options">The atomicity context options.</param>
    /// <param name="operationLog">The operation log.</param>
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected AtomicityContextBase()
      : this(AtomicityContextOptions.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="operationLog">The operation log.</param>
    protected AtomicityContextBase(IOperationLog operationLog)
      : this(AtomicityContextOptions.Default, operationLog)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="options">The atomicity context options.</param>
    protected AtomicityContextBase(AtomicityContextOptions options)
      : this(options, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="options">The atomicity context options.</param>
    /// <param name="operationLog">The operation log.</param>
    protected AtomicityContextBase(AtomicityContextOptions options, IOperationLog operationLog)
    {
      Initialize(options, operationLog);
    }
  }
}