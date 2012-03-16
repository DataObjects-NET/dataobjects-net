// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Aspects.Helpers;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Injects "initializable" aspect by modifying constructors so that
  /// methods with name <see cref="InitializeMethodName"/> and 
  /// <see cref="InitializationErrorMethodName"/> are invoked by each of them
  /// to ensure common post-construction initialization task is completed.
  /// </summary>
  /// <remarks>
  /// If you're really interested in actual behavior, we recommend you to
  /// study the decompiled MSIL code of class having this attribute applied 
  /// using .NET Reflector.
  /// </remarks>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Interface, 
    Inheritance = MulticastInheritance.Multicast, AllowMultiple = false, PersistMetaData = true)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ImplementConstructor))]
  [RequirePostSharp("Xtensive.Aspects.Weaver", "Xtensive.PlugIn")]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class InitializableAttribute : Aspect,
    IAspectProvider
  {
    public const string InitializeMethodName = "Initialize";
    public const string InitializationErrorMethodName = "InitializationError";

    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      var type = targetElement as Type;
      if (type == null)
        return result;

      // Getting all the base types
      var initializeMethodDeclarer = GetFirstMethodDeclarer(type, InitializeMethodName, new[] {typeof (Type)});
      if (initializeMethodDeclarer == null)
        return result;
      bool hasInitializationErrorHandler =
        GetMethod(initializeMethodDeclarer, InitializationErrorMethodName, new[] {typeof (Type), typeof (Exception)}) != null;

      // Applying the aspect to all the constructors
      var ctors = type.GetConstructors(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
      foreach (var ctor in ctors) {
        if (ctor.IsPrivate)
          continue;
        if (AspectHelper.IsInfrastructureMethod(ctor))
          continue;
        var constructorEpilogueAspect = hasInitializationErrorHandler
          ? new ImplementConstructorEpilogue(initializeMethodDeclarer, InitializeMethodName, InitializationErrorMethodName)
          : new ImplementConstructorEpilogue(initializeMethodDeclarer, InitializeMethodName);
        result.Add(new AspectInstance(ctor, constructorEpilogueAspect));
      }
      return result;
    }

    private static Type GetFirstMethodDeclarer(Type descendantType, string methodName, Type[] arguments)
    {
      // Looking for the first method declaration starting from the very base type
      var bases = new Stack<Type>();
      var type = descendantType;
      while (type!=typeof(object)) {
        bases.Push(type);
        type = type.BaseType;
      }

      foreach (Type currentBase in bases) {
        MethodInfo method = null;
        try {
          method = GetMethod(currentBase, methodName, arguments);
        }
        catch {}
        if (method != null)
          return currentBase;
      }
      return null;
    }

    private static MethodInfo GetMethod(Type type, string methodName, Type[] arguments)
    {
      return type.GetMethod(
        methodName,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding,
        null, arguments, null);
    }


    // Constructors

    public InitializableAttribute()
    {
      AttributeInheritance = MulticastInheritance.Multicast;
    }
  }
}