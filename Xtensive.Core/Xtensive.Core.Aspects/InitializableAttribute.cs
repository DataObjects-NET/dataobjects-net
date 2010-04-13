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
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Interface, Inheritance = MulticastInheritance.Multicast, AllowMultiple = false)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [Serializable]
  [RequirePostSharp("Xtensive.Core.Weaver", "Xtensive.PlugIn")]
  public sealed class InitializableAttribute : Aspect,
    IAspectProvider
  {
    public const string InitializeMethodName = "Initialize";
    public const string InitializationErrorMethodName = "InitializationError";

    #region IAspectProvider Members

    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      var type = targetElement as Type;
      if (type == null)
        return result;

      // Getting all the base types
      var initializeMethodDeclarer = FindFirstMethodDeclarer(type, InitializeMethodName, new[] {typeof (Type)});
      if (initializeMethodDeclarer == null)
        return result;
      bool hasInitializationErrorHandler =
        GetMethod(initializeMethodDeclarer, InitializationErrorMethodName, new[] {typeof (Type), typeof (Exception)}) !=
        null;

      // Applying the aspect to all the constructors
      foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
        if (constructor.IsPrivate)
          continue;
        var constructorEpilogueAspect = hasInitializationErrorHandler
          ? new ImplementConstructorEpilogue(initializeMethodDeclarer, InitializeMethodName, InitializationErrorMethodName)
          : new ImplementConstructorEpilogue(initializeMethodDeclarer, InitializeMethodName);
        result.Add(new AspectInstance(constructor, constructorEpilogueAspect));
      }
      return result;
    }

    #endregion

    private static Type FindFirstMethodDeclarer(Type descendantType, string methodName, Type[] arguments)
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
      AttributeInheritance = MulticastInheritance.Multicast;
    }
  }
}