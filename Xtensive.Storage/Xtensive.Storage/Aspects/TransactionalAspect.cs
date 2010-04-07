// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Activates session on method boundary.
  /// Opens the transaction, if this is necessary.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method | MulticastTargets.InstanceConstructor)]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, 
    AllowMultiple = false, Inherited = false)]
  [Serializable]
  internal sealed class TransactionalAspect : OnMethodBoundaryAspect
  {
    private bool openSession = true;
    private bool openTransaction = true;
    private TransactionOpenMode mode = TransactionOpenMode.Auto;

    /// <summary>
    /// Gets or sets a value indicating whether a <see cref="Session"/>
    /// must be opened for the method this aspect is applied to.
    /// Default value is <see langword="true" />.
    /// </summary>
    public bool OpenSession
    {
      get { return openSession; }
      set { openSession = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a <see cref="Transaction"/>
    /// must be opened for the method this aspect is applied to.
    /// Default value is <see langword="true" />.
    /// </summary>
    public bool OpenTransaction
    {
      get { return openTransaction; }
      set { openTransaction = value; }
    }

    /// <summary>
    /// Gets or sets value describing transaction opening mode.
    /// Default value is <see cref="TransactionOpenMode.Auto"/>.
    /// </summary>
    public TransactionOpenMode Mode
    {
      get { return mode; }
      set { mode = value; }
    }


    public override bool CompileTimeValidate(MethodBase method)
    {
      if (!AspectHelper.ValidateContextBoundMethod<Session>(this, method, true, false))
        return false;

      if (!AspectHelper.ValidateNotInfrastructure(this, method))
        return false;

      return true;
    }
//
//    public static TransactionalAspect ApplyOnce(MethodBase method, 
//      bool openSession, bool openTransaction, TransactionOpenMode mode)
//    {
//      ArgumentValidator.EnsureArgumentNotNull(method, "method");
//
//      return AppliedAspectSet.Add(method, 
//        () => new TransactionalAspect {
//          OpenSession = openSession, 
//          OpenTransaction = openTransaction,
//          Mode = mode
//        });
//    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Session switching is detected.</exception>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionArgs args)
    {
      var sessionBound = (ISessionBound) args.Instance;
      var session = sessionBound.Session;
      IDisposable sessionScope = null;
      if (openSession)
        sessionScope = session.Activate(true);
      if (!openTransaction)
        args.MethodExecutionTag = sessionScope;
      else {
        var transactionScope = session == null
          ? Transaction.Open(mode)
          : Transaction.Open(session, mode);
        args.MethodExecutionTag = transactionScope.Join(sessionScope);
      }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnSuccess(MethodExecutionArgs args)
    {
      if (!openTransaction)
        return;
      if (args.MethodExecutionTag == null) // The most probable case : inner call, nothing was opened
        return;
      var transactionScope = args.MethodExecutionTag as TransactionScope;
      if (transactionScope!=null) { // 2nd probable case : only transaction was opened
        transactionScope.Complete();
        return;
      }
      var joiningDisposable = args.MethodExecutionTag as JoiningDisposable;
      if (joiningDisposable!=null) { // Less probable case : session & trans. were opened
        transactionScope = joiningDisposable.First as TransactionScope;
        transactionScope.Complete();
      }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionArgs args)
    {
      var disposable = (IDisposable) args.MethodExecutionTag;
      disposable.DisposeSafely();
    }
  }
}