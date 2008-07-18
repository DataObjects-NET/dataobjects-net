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
using Xtensive.Core.Aspects;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Implements epilogue call in constructor.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementConstructorEpilogueAspect : LaosMethodLevelAspect
  {
    /// <summary>
    /// Gets the type where epilogue method is declared.
    /// </summary>
    public Type HandlerType { get; private set; }

    /// <summary>
    /// Gets the name of the epilogue method to call.
    /// </summary>
    public string HandlerMethodName { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      var ctorInfo = method as ConstructorInfo;
      if (ctorInfo == null) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExCanBeAppliedToConstructorOnly",
          new object[] { GetType().Name, method.DeclaringType.FullName });
        return false;
      }

      if (ctorInfo.IsStatic) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExCannotBeAppliedToStaticMember",
          new object[] { GetType().Name, method.DeclaringType.FullName });
        return false;
      }

      MethodInfo targetMethodInfo = null;
      try {
        targetMethodInfo = HandlerType.GetMethod(HandlerMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      }
      catch (NullReferenceException) {}
      catch (ArgumentNullException) {}
      catch (AmbiguousMatchException) {}
      if (targetMethodInfo==null) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExNoMethod",
          new object[] {GetType().Name, method.DeclaringType.FullName, HandlerType.FullName, HandlerMethodName});
        return false;
      }

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
    /// Applies this aspect to the specified <paramref name="ctor"/>.
    /// </summary>
    /// <param name="ctor">The property getter or setter to apply the aspect to.</param>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodName"><see cref="HandlerMethodName"/> property value.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ImplementAutoPropertyReplacementAspect ApplyOnce(ConstructorInfo ctor, Type handlerType, string handlerMethodName)
    {
      ArgumentValidator.EnsureArgumentNotNull(ctor, "getterOrSetter");
      ArgumentValidator.EnsureArgumentNotNull(handlerType, "handlerType");
      ArgumentValidator.EnsureArgumentNotNull(handlerMethodName, "handlerMethodName");

      return AppliedAspectSet.Add(new Triplet<ConstructorInfo, Type, string>(ctor, handlerType, handlerMethodName), 
        () => new ImplementAutoPropertyReplacementAspect(handlerType, handlerMethodName));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlerType"><see cref="HandlerType"/> property value.</param>
    /// <param name="handlerMethodName"><see cref="HandlerMethodName"/> property value.</param>
    public ImplementConstructorEpilogueAspect(Type handlerType, string handlerMethodName)
    {
      HandlerType = handlerType;
      HandlerMethodName = handlerMethodName;
    }
  }
}