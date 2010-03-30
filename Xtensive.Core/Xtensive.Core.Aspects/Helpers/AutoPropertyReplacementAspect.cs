// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Replaces auto-property implementation to invocation of property get and set generic handlers.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  [RequirePostSharp("Xtensive.Core.Weaver", "Xtensive.PlugIn")]
  public sealed class AutoPropertyReplacementAspect : MethodLevelAspect
  {
    /// <summary>
    /// Gets the type where handlers are declared.
    /// </summary>
    public Type HandlerType { get; private set; }


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
      var accessorInfo = method as MethodInfo;
      if (accessorInfo == null) {
        ErrorLog.Write(SeverityType.Error, AspectMessageType.AspectRequiresToBe,
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(method.DeclaringType, method),
          string.Empty,
          Strings.PropertyAccessor);
        return false;
      }

      if (null==AspectHelper.ValidateMemberAttribute<CompilerGeneratedAttribute>(this, SeverityType.Error,
        method, true, AttributeSearchOptions.Default))
        return false;

      return true;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodSuffix"><see cref="HandlerMethodSuffix"/> property value.</param>
    public AutoPropertyReplacementAspect(Type handlerType, string handlerMethodSuffix)
    {
      HandlerType = handlerType;
      HandlerMethodSuffix = handlerMethodSuffix;
    }
  }
}