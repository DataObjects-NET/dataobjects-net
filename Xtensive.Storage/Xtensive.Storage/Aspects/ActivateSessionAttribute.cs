using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage
{
  /// <summary>
  /// Activates session or disables its activation on method boundary.
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
  [ProvideAspectRole(StandardRoles.Persistence)]
  [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class ActivateSessionAttribute : OnMethodBoundaryAspect
  {
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
    }

    public override bool CompileTimeValidate(MethodBase method)
    {
      if (AspectHelper.IsInfrastructureMethod(method))
        return false;
      var transactionalAttribute = method.GetAttribute<TransactionalAttribute>(AttributeSearchOptions.InheritFromPropertyOrEvent);
      if (transactionalAttribute != null)
        return false;
      return ActivateSession;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Session switching is detected.</exception>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionArgs args)
    {
      var sessionBound = (ISessionBound) args.Instance;
      var session = sessionBound.Session;
      args.MethodExecutionTag = session.Activate(true);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionArgs args)
    {
      var disposable = (IDisposable) args.MethodExecutionTag;
      disposable.DisposeSafely();
    }


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActivateSessionAttribute()
      : this (true)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActivateSessionAttribute(bool activateSession)
    {
      ActivateSession = activateSession;
      base.AttributePriority = 2;
    }

    internal ActivateSessionAttribute(ActivateSessionTypeAttribute activateSessionTypeAttribute)
    {
      ActivateSession = true;
      base.AttributePriority = 1;
    }
  }
}