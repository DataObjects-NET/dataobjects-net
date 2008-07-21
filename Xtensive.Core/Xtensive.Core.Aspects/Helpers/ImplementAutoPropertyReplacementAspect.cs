// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Replaces auto-property implementation to invocation of property get and set generic handlers.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementAutoPropertyReplacementAspect : LaosMethodLevelAspect
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
    public override bool  CompileTimeValidate(MethodBase method)
    {
      // TODO: Implement
      return true;
    }

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Core.Weaver");
      requirements.Tasks.Add("Xtensive.Core.Weaver.WeaverFactory");
      return requirements;
    }

    /// <summary>
    /// Applies this aspect to the specified <paramref name="getterOrSetter"/>.
    /// </summary>
    /// <param name="getterOrSetter">The property getter or setter to apply the aspect to.</param>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodSuffix"><see cref="HandlerMethodSuffix"/> property value.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ImplementAutoPropertyReplacementAspect ApplyOnce(MethodInfo getterOrSetter, Type handlerType, string handlerMethodSuffix)
    {
      ArgumentValidator.EnsureArgumentNotNull(getterOrSetter, "getterOrSetter");
      ArgumentValidator.EnsureArgumentNotNull(handlerType, "handlerType");
      ArgumentValidator.EnsureArgumentNotNull(handlerMethodSuffix, "handlerMethodSuffix");

      return AppliedAspectSet.Add(getterOrSetter, 
        () => new ImplementAutoPropertyReplacementAspect(handlerType, handlerMethodSuffix));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodSuffix"><see cref="HandlerMethodSuffix"/> property value.</param>
    public ImplementAutoPropertyReplacementAspect(Type handlerType, string handlerMethodSuffix)
    {
      HandlerType = handlerType;
      HandlerMethodSuffix = handlerMethodSuffix;
    }
  }
}