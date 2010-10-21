// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29
// Copyright (C) 2003-2010 Xtensive LLC.


using System;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Aspects.Helpers;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Injects epilogue method calls into the constructor.
  /// </summary>
  /// <remarks>
  /// If you're really interested in actual behavior, we recommend you to
  /// study the decompiled MSIL code of class having this attribute applied 
  /// using .NET Reflector.
  /// </remarks>
  [MulticastAttributeUsage(MulticastTargets.InstanceConstructor)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  [RequirePostSharp("Xtensive.Aspects.Weaver", "Xtensive.PlugIn")]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class ImplementConstructorEpilogue : MethodLevelAspect
  {
    /// <summary>
    /// Gets the type where epilogue method is declared.
    /// </summary>
    public Type HandlerType { get; private set; }

    /// <summary>
    /// Gets the name of the epilogue method to call.
    /// </summary>
    public string HandlerMethodName { get; private set; }

    /// <summary>
    /// Gets the name of the initialization error handler method to call.
    /// </summary>
    public string ErrorHandlerMethodName { get; protected set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (AspectHelper.IsInfrastructureMethod(method))
        return false;
      if (!AspectHelper.ValidateMemberType(this, SeverityType.Error,
        method, true, MemberTypes.Constructor))
        return false;
      if (!AspectHelper.ValidateMethodAttributes(this, SeverityType.Error,
        method, false, MethodAttributes.Static))
        return false;
      if (!AspectHelper.ValidateBaseType(this, SeverityType.Error,
        method.DeclaringType, true, HandlerType))
        return false;

      MethodInfo targetMethod; 
      
      if (!AspectHelper.ValidateMethod(this, SeverityType.Error,
        HandlerType, true, 
        BindingFlags.Instance |
        BindingFlags.DeclaredOnly |
        BindingFlags.Public | 
        BindingFlags.NonPublic | 
        BindingFlags.ExactBinding,
        typeof(void), HandlerMethodName, new [] { typeof (Type) }, out targetMethod))
        return false;
      if (!AspectHelper.ValidateMethodAttributes(this, SeverityType.Error,
        targetMethod, false, MethodAttributes.Private))
        return false;
      if (!AspectHelper.ValidateMethodAttributes(this, SeverityType.Error,
        targetMethod, false, MethodAttributes.Virtual))
        return false;

      if (!ErrorHandlerMethodName.IsNullOrEmpty()) {
        if (!AspectHelper.ValidateMethod(this, SeverityType.Error,
          HandlerType, true,
          BindingFlags.Instance |
          BindingFlags.DeclaredOnly |
          BindingFlags.Public |
          BindingFlags.NonPublic |
          BindingFlags.ExactBinding,
          typeof (void), ErrorHandlerMethodName, new[] {typeof (Type), typeof (Exception)}, out targetMethod))
          return false;
        if (!AspectHelper.ValidateMethodAttributes(this, SeverityType.Error,
          targetMethod, false, MethodAttributes.Private))
          return false;
        if (!AspectHelper.ValidateMethodAttributes(this, SeverityType.Error,
          targetMethod, false, MethodAttributes.Virtual))
          return false;
      }

      return true;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodName"><see cref="HandlerMethodName"/> property value.</param>
    public ImplementConstructorEpilogue(Type handlerType, string handlerMethodName)
      : this(handlerType, handlerMethodName, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodName"><see cref="HandlerMethodName"/> property value.</param>
    /// <param name="errorHandlerMethodName"><see cref="ErrorHandlerMethodName"/> property value.</param>
    public ImplementConstructorEpilogue(Type handlerType, string handlerMethodName, string errorHandlerMethodName)
    {
      HandlerType = handlerType;
      HandlerMethodName = handlerMethodName;
      ErrorHandlerMethodName = errorHandlerMethodName;
    }
  }
}