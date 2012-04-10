// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Aspects;

using Xtensive.Reflection;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm
{
  /// <summary>
  /// Activates session on method boundary for <see cref="ISessionBound"/> implementors on public instance methods.
  /// Opens the transaction for public methods.
  /// </summary>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Multicast, AllowMultiple = false,
    TargetMemberAttributes =
      MulticastAttributes.Instance |
      MulticastAttributes.UserGenerated |
      MulticastAttributes.Public |
      MulticastAttributes.Managed |
      MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
  [ProvideAspectRole(StandardRoles.TransactionHandling)]
  [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof (ReplaceAutoProperty))]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class TransactionalTypeAttribute : Aspect, IAspectProvider
  {
    internal bool? activateSession;

    /// <summary>
    /// Gets or sets value describing transaction opening mode.
    /// Default value is <see cref="TransactionalBehavior.Auto"/>.
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

    
    public override bool CompileTimeValidate(object target)
    {
      var member = target as MemberInfo;
      if (member != null) {
        var transactionalAttribute = member.GetAttribute<TransactionalAttribute>(AttributeSearchOptions.InheritFromPropertyOrEvent);
        if (transactionalAttribute != null)
          return false;
      }
      return true;
    }

    
    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      yield return new AspectInstance(targetElement, new TransactionalAttribute(this));
    }

    #region Constructors

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public TransactionalTypeAttribute()
      : this(TransactionalBehavior.Open)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="mode">The transactional behavior.</param>
    public TransactionalTypeAttribute(TransactionalBehavior mode)
    {
      Mode = mode;
      activateSession = null;
    }

    #endregion
  }
}