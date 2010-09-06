// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Disposing;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage
{
  /// <summary>
  /// Activates session on method boundary.
  /// Opens the transaction, if this is necessary.
  /// </summary>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.None, AllowMultiple = false,
    TargetMemberAttributes = 
      MulticastAttributes.AnyGeneration |
      MulticastAttributes.AnyScope |
      MulticastAttributes.AnyVisibility |
      MulticastAttributes.Managed | 
      MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  [ProvideAspectRole(StandardRoles.TransactionHandling)]
  [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class TransactionalAttribute : OnMethodBoundaryAspect
  {
    /// <summary>
    /// Gets or sets value describing transaction opening mode.
    /// Default value is <see cref="TransactionOpenMode.Auto"/>.
    /// </summary>
    public TransactionalBehavior Mode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a session should be activated on the method boundaries.
    /// </summary>
    public bool ActivateSession { get; set; }

    #region Hide base properties
    // ReSharper disable UnusedMember.Local
    private new bool AttributeId
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AspectPriority
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeExclude
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeInheritance
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributePriority
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeReplace
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeTargetAssemblies
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeTargetElements
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeTargetMemberAttributes
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeTargetMembers
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeTargetParameterAttributes
    {
      get { throw new NotSupportedException(); }
    }

    private new bool AttributeTargetParameters
    {
      get { throw new NotSupportedException(); }
    }
    private new bool AttributeTargetTypeAttributes
    {
      get { throw new NotSupportedException(); }
    }
    private new bool AttributeTargetTypes
    {
      get { throw new NotSupportedException(); }
    }

    // ReSharper restore UnusedMember.Local
    #endregion
    
    public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
    {
      ActivateSession &= typeof (ISessionBound).IsAssignableFrom(method.DeclaringType) && !method.IsStatic;
      if (ActivateSession && AspectHelper.ContextActivationIsSuppressed(method, typeof (Session)))
        ActivateSession = false;
      if (Mode != TransactionalBehavior.Suppress) {
        var nonTransactionalAttribute = method.GetAttribute<NonTransactionalAttribute>(AttributeSearchOptions.InheritFromPropertyOrEvent);
        if (nonTransactionalAttribute != null)
          Mode = TransactionalBehavior.Suppress;
      }
    }

    public override bool CompileTimeValidate(MethodBase method)
    {
      if (AspectHelper.IsInfrastructureMethod(method))
        return false;
      return ActivateSession || Mode != TransactionalBehavior.Suppress;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Session switching is detected.</exception>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionArgs args)
    {
      var sessionBound = args.Instance as ISessionBound;
      var session = sessionBound == null
        ? Session.Demand()
        : sessionBound.Session;
      IDisposable sessionScope = null;
      if (ActivateSession)
        sessionScope = session.Activate(true);
      var transactionScope = Transaction.HandleAutoTransaction(session, Mode);
      args.MethodExecutionTag = transactionScope.IsVoid
        ? sessionScope
        : (sessionScope == null
              ? (IDisposable) transactionScope
              : transactionScope.Join(sessionScope));
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnSuccess(MethodExecutionArgs args)
    {
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
      : this (TransactionalBehavior.Open)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mode">The transaction opening mode.</param>
    public TransactionalAttribute(TransactionalBehavior mode)
    {
      Mode = mode;
      ActivateSession = true;
      base.AttributePriority = 2;
    }

    internal TransactionalAttribute(TransactionalTypeAttribute transactionalTypeAttribute)
    {
      Mode = transactionalTypeAttribute.Mode;
      ActivateSession = transactionalTypeAttribute.ActivateSession;
      base.AttributePriority = 1;
    }
  }
}