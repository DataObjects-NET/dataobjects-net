// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.19

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage
{
  /// <summary>
  /// Activates session on method boundary for <see cref="ISessionBound"/> implementors on public instance methods.
  /// </summary>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Multicast, AllowMultiple = false,
    TargetMemberAttributes =
      MulticastAttributes.Instance |
      MulticastAttributes.UserGenerated |
      MulticastAttributes.Public |
      MulticastAttributes.Managed |
      MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [ProvideAspectRole(StandardRoles.Persistence)]
  [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(ActivateSessionAttribute))]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
#if NET40
  [SecuritySafeCritical]
#endif
  internal sealed class ActivateSessionTypeAttribute: Aspect, IAspectProvider
  {
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

    /// <inheritdoc/>
    public override bool CompileTimeValidate(object target)
    {
      var member = target as MemberInfo;
      if (member != null) {
        var activateSessionAttribute = member.GetAttribute<ActivateSessionAttribute>(AttributeSearchOptions.InheritFromPropertyOrEvent);
        if (activateSessionAttribute != null)
          return false;
        var transactionalAttribute = member.GetAttribute<TransactionalAttribute>(AttributeSearchOptions.InheritFromPropertyOrEvent);
        if (transactionalAttribute != null)
          return false;
      }
      return true;
    }

    /// <inheritdoc/>
    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      yield return new AspectInstance(targetElement, new ActivateSessionAttribute(this));
    }

    #region Constructors

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActivateSessionTypeAttribute()
    {
    }

    #endregion
  }
}