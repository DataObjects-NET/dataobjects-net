// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Aspects.Helpers;
using Xtensive.Aspects.Resources;
using System.Linq;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Replaces auto-property implementation to invocation of property get and set generic handlers.
  /// </summary>
  /// <remarks>
  /// If you're really interested in actual behavior, we recommend you to
  /// study the decompiled MSIL code of class having this attribute applied 
  /// using .NET Reflector.
  /// </remarks>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Multicast)]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(NotSupportedAttribute))]
  [RequirePostSharp("Xtensive.Aspects.Weaver", "Xtensive.PlugIn")]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class ReplaceAutoProperty : MethodLevelAspect
  {
    /// <summary>
    /// Gets the name suffix of handler methods.
    /// </summary>
    /// <remarks>
    /// If suffix is "Value", handler methods should be
    /// <c>T GetValue&lt;T&gt;(string name)</c> and
    /// <c>void SetValue&lt;T&gt;(string name, T value)</c>.
    /// </remarks>
    public string HandlerMethodSuffix { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (AspectHelper.IsInfrastructureMethod(method))
        return false;
      var accessorInfo = method as MethodInfo;
      if (accessorInfo == null) {
        ErrorLog.Write(SeverityType.Error, AspectMessageType.AspectRequiresToBe,
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(method.DeclaringType, method),
          string.Empty,
          Strings.PropertyAccessor);
        return false;
      }

      var isCompilerGenerated = accessorInfo.IsDefined<CompilerGeneratedAttribute>();
      var isDebuggerNonUserCode = accessorInfo.IsDefined<DebuggerNonUserCodeAttribute>();

      var result = isCompilerGenerated || isDebuggerNonUserCode;
      if (!result) {
        // Validation fo VB
        var property = method.ReflectedType.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
          delegate(MemberInfo mi, object state) {
              var getOrSet = (MethodInfo)state;
              var pi = mi as PropertyInfo;
              if (pi == null)
                return false;

              return pi.GetGetMethod(true) == getOrSet ||
                      pi.GetSetMethod(true) == getOrSet;
            }, accessorInfo)
          .SingleOrDefault();
        if (property != null) {
          var field = method.ReflectedType.FindMembers(MemberTypes.Field, BindingFlags.Instance | BindingFlags.NonPublic, Type.FilterName, string.Format("_{0}", property.Name))
            .SingleOrDefault();
          if (field != null)
            result = field.IsDefined<CompilerGeneratedAttribute>();
        }
      }
      return result;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceAutoProperty"/> class.
    /// </summary>
    /// <param name="handlerMethodSuffix"><see cref="HandlerMethodSuffix"/> property value.</param>
    public ReplaceAutoProperty(string handlerMethodSuffix)
    {
      HandlerMethodSuffix = handlerMethodSuffix;
      AttributeTargetMemberAttributes = 
        MulticastAttributes.CompilerGenerated | 
        MulticastAttributes.Managed | 
        MulticastAttributes.NonAbstract | 
        MulticastAttributes.Instance;
    }
  }
}