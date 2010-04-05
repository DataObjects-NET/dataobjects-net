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
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Interface, Inheritance = MulticastInheritance.Multicast, AllowExternalAssemblies = false, AllowMultiple = false)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [Serializable]
  public sealed class InitializableAttribute : Aspect,
    IAspectProvider
  {
    public const string InitializeMethodName = "Initialize";
    public const string InitializationErrorMethodName = "InitializationError";

    #region IAspectProvider Members

    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var type = targetElement as Type;
      if (type == null)
        yield break;

      // Getting all the base types
      Type initializeMethodDeclarer = FindFirstMethodDeclarer(type, InitializeMethodName, new[] {typeof (Type)});
      if (initializeMethodDeclarer == null)
        yield break;
      bool hasInitializationErrorHandler =
        GetMethod(initializeMethodDeclarer, InitializationErrorMethodName, new[] {typeof (Type), typeof (Exception)}) !=
        null;

      // Applying the aspect to all the constructors
      foreach (ConstructorInfo constructor in type.GetConstructors()) {
        if (!constructor.IsPublic && !IsDefined(constructor, typeof (DebuggerNonUserCodeAttribute)))
          continue;
        var constructorEpilogueAspect = hasInitializationErrorHandler
          ? new ConstructorEpilogueAspect(initializeMethodDeclarer, InitializeMethodName, InitializationErrorMethodName)
          : new ConstructorEpilogueAspect(initializeMethodDeclarer, InitializeMethodName);
        yield return new AspectInstance(constructor, constructorEpilogueAspect);
      }
    }

    #endregion

    private Type FindFirstMethodDeclarer(Type descendantType, string methodName, Type[] arguments)
    {
      // Looking for the first method declaration 
      // starting from the very base type
      IEnumerable<Type> bases =
        EnumerableUtils.Unfold(descendantType, type => type.BaseType)
          .Reverse()
          .Skip(1); // Skipping object type
      foreach (Type currentBase in bases)
      {
        MethodInfo method = null;
        try
        {
          method = GetMethod(currentBase, methodName, arguments);
        }
        catch
        {}
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
    }
  }
}