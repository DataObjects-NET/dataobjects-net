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
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Activates session on method boundary.
  /// Opens the transaction, if this is necessary.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Multicast, AllowMultiple = false,
    TargetMemberAttributes = 
      MulticastAttributes.Instance |
      MulticastAttributes.Static |
      MulticastAttributes.UserGenerated | 
      MulticastAttributes.Managed | 
      MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  [Serializable]
  public sealed class TransactionalAttribute : OnMethodBoundaryAspect
  {
    [NonSerialized]
    private bool activateSession;
    [NonSerialized]
    private bool openTransaction;
    [NonSerialized]
    private TransactionOpenMode mode;

    /// <summary>
    /// Gets or sets value describing transaction opening mode.
    /// Default value is <see cref="TransactionOpenMode.Auto"/>.
    /// </summary>
    public TransactionOpenMode Mode
    {
      get { return mode; }
      set { mode = value; }
    }

    public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
    {
      activateSession = typeof (ISessionBound).IsAssignableFrom(method.DeclaringType) && !method.IsStatic;
      openTransaction = true;
      var activateSessionAttribute = method.GetAttribute<ActivateSessionAttribute>(AttributeSearchOptions.InheritAll);
      var nonTransactionalAttribute = method.GetAttribute<NonTransactionalAttribute>(AttributeSearchOptions.InheritAll);
      if (activateSessionAttribute != null)
        activateSession &= activateSessionAttribute.Activate;
      if (nonTransactionalAttribute != null)
        openTransaction = false;
    }

    public override bool CompileTimeValidate(MethodBase method)
    {
      if (AspectHelper.IsInfrastructureMethod(method))
        return false;
      return activateSession || activateSession;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Session switching is detected.</exception>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionArgs args)
    {
      var sessionBound = (ISessionBound) args.Instance;
      var session = sessionBound.Session;
      IDisposable sessionScope = null;
      if (activateSession)
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TransactionalAttribute()
      : this (TransactionOpenMode.Auto)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mode">The transaction opening mode.</param>
    public TransactionalAttribute(TransactionOpenMode mode)
    {
      this.mode = mode;
    }
  }
}