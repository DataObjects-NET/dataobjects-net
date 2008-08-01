// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using System.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public class InitializableAttribute : CompoundAspect
  {
    public const string InitializeMethodName = "Initialize";

    // TODO: Add CompileTimeValidate

    /// <inheritdoc/>
    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      Type type = element as Type;
      if (type==null)
        return;

      // Getting all the base types
      var bases = new List<Type>();
      Type current = type;
      while (current!=typeof(object)) {
        bases.Add(current);
        current = current.BaseType;
      }

      // Looking for the first "Initialize" method declaration 
      // starting from the very base type
      MethodInfo initializeMethod = null;
      Type initializeMethodType   = null;
      for (int i = bases.Count - 1; i >= 0; i--) {
        var currentBase = bases[i];
        try {
          initializeMethod = currentBase.GetMethod(InitializeMethodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] {typeof (Type)}, null);
        }
        catch {}
        if (initializeMethod!=null) {
          initializeMethodType = currentBase;
          break;
        }
      }
      if (initializeMethodType==null)
        return;

      // Applying the aspect to all the constructors
      foreach (var constructor in type.GetConstructors()) {
        var icea = ImplementConstructorEpilogueAspect.ApplyOnce(constructor, initializeMethodType, InitializeMethodName);
        if (icea!=null)
          collection.AddAspect(constructor, icea);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public InitializableAttribute()
    {
    }
  }
}