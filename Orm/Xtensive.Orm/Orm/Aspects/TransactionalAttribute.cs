// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Transactions;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Aspects;
using Xtensive.Aspects.Helpers;
using Xtensive.Disposing;
using Xtensive.Core;

using Xtensive.Reflection;
using Xtensive.Orm.Configuration;


namespace Xtensive.Orm
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
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  [ProvideAspectRole(StandardRoles.TransactionHandling)]
  [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class TransactionalAttribute : OnMethodBoundaryAspect
  {
    private bool? activateSession;

    /// <summary>
    /// Gets or sets value describing transaction opening mode.
    /// Default value is <see cref="TransactionOpenMode.Auto"/>.
    /// </summary>
    public TransactionalBehavior Mode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a session should be activated on the method boundaries.
    ///  </summary>
    /// <remarks>When the value is not set explicitely actual value will be resolved according to 
    /// <see cref="SessionOptions.AutoActivation"/> flag of the current session.</remarks>
    public bool ActivateSession
    {
      get { return activateSession.GetValueOrDefault(); }
      set { activateSession = value; }
    }

    /// <inheritdoc/>
    public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
    {
      var canActivate = typeof (ISessionBound).IsAssignableFrom(method.DeclaringType) 
        && !method.IsStatic
        && !ContextActivationIsSuppressed(method, typeof(Session));
      if (!canActivate) 
        ActivateSession = false;
      if (Mode == TransactionalBehavior.Suppress) 
        return;
      var nonTransactionalAttribute = method.GetAttribute<NonTransactionalAttribute>(AttributeSearchOptions.InheritFromPropertyOrEvent);
      if (nonTransactionalAttribute != null)
        Mode = TransactionalBehavior.Suppress;
    }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (AspectHelper.IsInfrastructureMethod(method))
        return false;
      if (Mode != TransactionalBehavior.Suppress)
        return true;
      return activateSession == null || activateSession.Value;
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
      var activate = activateSession.HasValue
        ? activateSession.Value
        : session.Configuration.Supports(SessionOptions.AutoActivation);
      if (activate)
        sessionScope = session.Activate(true);
      var tx = session.OpenAutoTransaction(Mode);
      args.MethodExecutionTag = tx.IsVoid
        ? sessionScope
        : (sessionScope == null ? (IDisposable) tx : tx.Join(sessionScope));
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
        transactionScope = (TransactionScope)joiningDisposable.First;
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

    private static bool ContextActivationIsSuppressed(MethodBase method, Type contextType)
    {
      var attributes = method
        .GetAttributes<SuppressActivationAttribute>(AttributeSearchOptions.Default)
        .Concat(method.DeclaringType.GetAttributes<SuppressActivationAttribute>(AttributeSearchOptions.InheritNone))
        .ToList();
      if (attributes.Count == 0)
        return false;
      return attributes
        .Any(attribute => attribute.ContextType == contextType);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public TransactionalAttribute()
      : this (TransactionalBehavior.Open)
    {}

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="mode">The transaction opening mode.</param>
    public TransactionalAttribute(TransactionalBehavior mode)
    {
      Mode = mode;
      activateSession = null;
    }

    internal TransactionalAttribute(TransactionalTypeAttribute transactionalTypeAttribute)
    {
      Mode = transactionalTypeAttribute.Mode;
      activateSession = transactionalTypeAttribute.activateSession;
    }
  }
}