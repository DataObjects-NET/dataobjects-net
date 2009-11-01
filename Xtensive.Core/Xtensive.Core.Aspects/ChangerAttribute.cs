// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.20

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Internals;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Property | MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ChangerAttribute : CompoundAspect
  {
    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      PropertyInfo pi = element as PropertyInfo;
      if (pi!=null)
        return; // Will be anyway called for get\set methods with MethodInfo element further
      MethodInfo mi = (MethodInfo)element;
      Type t = mi.DeclaringType;
      bool isNew;
      ImplementChangeNotifierAspect icna = ImplementChangeNotifierAspect.FindOrCreate(t, out isNew);
      if (isNew)
        collection.AddAspect(t, icna);
      // AspectDebug.WriteLine("Providing ImplementChangerAspect for {0}.{1}", t.Name, mi.Name);
      collection.AddAspect(mi, new ImplementChangerAspect(this));
    }


    // Constructors

    public ChangerAttribute()
    {
    }
  }
}