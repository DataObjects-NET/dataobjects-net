// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using System.Linq;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [Serializable]
  public sealed class InitializableAttribute : Aspect
  {
    public const string InitializeMethodName = "Initialize";
    public const string InitializationErrorMethodName = "InitializationError";

    public override bool CompileTimeValidate(object element)
    {
      var type = element as Type;
      if (type == null)
        return false;

      // Let's ignore the types that aren't marked by IInitializable
      if (!typeof(IInitializable).IsAssignableFrom(type))
        return false; 

      return true;
    }

    /// <inheritdoc/>
//    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
//    {
//      Type type = element as Type;
//      if (type==null)
//        return;
//
      // Getting all the base types
//      Type initializeMethodDeclarer = FindFirstMethodDeclarer(type, 
//        InitializeMethodName, 
//        new[] {typeof (Type)});
//      if (initializeMethodDeclarer==null)
//        return;
//      bool hasInitializationErrorHandler = 
//        GetMethod(initializeMethodDeclarer, 
//          InitializationErrorMethodName,
//          new[] {typeof (Type), typeof(Exception)})!=null;
//
      // Applying the aspect to all the constructors
//      foreach (var constructor in type.GetConstructors()) {
//        if (!constructor.IsPublic && !IsDefined(constructor, typeof(DebuggerNonUserCodeAttribute)))
//          continue;
//        var icea = hasInitializationErrorHandler 
//          ? ConstructorEpilogueAspect.ApplyOnce(constructor, initializeMethodDeclarer, InitializeMethodName, InitializationErrorMethodName)
//          : ConstructorEpilogueAspect.ApplyOnce(constructor, initializeMethodDeclarer, InitializeMethodName);
//        if (icea!=null)
//          collection.AddAspect(constructor, icea);
//      }
//    }

    private Type FindFirstMethodDeclarer(Type descendantType, string methodName, Type[] arguments)
    {
      // Looking for the first method declaration 
      // starting from the very base type
      var bases =
        EnumerableUtils.Unfold(descendantType, type => type.BaseType)
          .Reverse()
          .Skip(1); // Skipping object type
      foreach (var currentBase in bases) {
        MethodInfo method = null;
        try {
          method = GetMethod(currentBase, methodName, arguments);
        }
        catch {}
        if (method!=null)
          return currentBase;
      }
      return null;
    }

    private static MethodInfo GetMethod(Type type, string methodName, Type[] arguments)
    {
      return type.GetMethod(methodName,
        BindingFlags.Instance | 
          BindingFlags.Public | 
            BindingFlags.NonPublic |
              BindingFlags.ExactBinding,
        null, arguments, null);
    }

//    public override PostSharpRequirements GetPostSharpRequirements()
//    {
//      PostSharpRequirements requirements = base.GetPostSharpRequirements();
//      AspectHelper.AddStandardRequirements(requirements);
//      return requirements;
//    }
//

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public InitializableAttribute()
    {
    }
  }
}