// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.20

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Aspects.Helpers.Internals;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Property | MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ChangerAttribute : CompoundAspect
  {
    // TODO: Add CompileTimeValidate

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      var propertyInfo = element as PropertyInfo;
      if (propertyInfo!=null)
        return; // Will be anyway called for get\set methods with MethodInfo element further
      var methodInfo = (MethodInfo) element;
      var type = methodInfo.DeclaringType;
      var icna = ImplementChangeNotifierAspect.ApplyOnce(type);
      if (icna!=null)
        collection.AddAspect(type, icna);
      // AspectDebug.WriteLine("Providing NotifyOnChangeAspect for {0}.{1}", t.Name, mi.Name);
      collection.AddAspect(methodInfo, new NotifyOnChangeAspect(this));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ChangerAttribute()
    {
    }
  }
}