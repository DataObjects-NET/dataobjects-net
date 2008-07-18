// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Attributes;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [Serializable]
  public sealed class StorageAttribute : CompoundAspect
  {
    private static readonly Type persistentType = typeof(Persistent);
    private static readonly Type entityType     = typeof(Entity);
    private static readonly Type structureType  = typeof(Structure);

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      Type type = element as Type;

      if (type == null)
        return;

      if (typeof(IContextBound<Session>).IsAssignableFrom(type))
        ProvideSessionBoundAspects(type, collection);

      if (type.IsSubclassOf(persistentType))
        ProvidePersistentAspects(type, collection);
    }

    private void ProvideSessionBoundAspects(Type type, LaosReflectionAspectCollection collection)
    {
      foreach (MethodInfo mi in type.GetMethods(
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.DeclaredOnly))
      {
        SuppressActivationAttribute fieldAttribute = mi.GetAttribute<SuppressActivationAttribute>(false);
        if (fieldAttribute != null && 
          (fieldAttribute.ContextType == null || 
          fieldAttribute.ContextType == typeof(Session)))
          continue;

        if (type.IsAbstract && mi.IsAbstract && mi.GetMethodBody() == null)
          continue;

        collection.AddAspect(mi, new SessionBoundMethodAspect());
      }
    }

    private void ProvidePersistentAspects(Type type, LaosReflectionAspectCollection collection)
    {
      Type baseType = ProvideConstructorDelegateAspect(type, collection);

      ProvideConstructorAspect(type, collection, baseType);

      ProvideAutoPropertyAspects(type, collection);
    }

    private void ProvideAutoPropertyAspects(Type type, LaosReflectionAspectCollection collection)
    {
      foreach (PropertyInfo pi in type.GetProperties(
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.DeclaredOnly)) 
      {

        try {
          FieldAttribute fieldAttribute = pi.GetAttribute<FieldAttribute>(true);
          if (fieldAttribute == null)
            continue;
        }
        catch (InvalidOperationException) {
          AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExMultipleAttributesOfTypeXAreNotAllowedHere",
                                              new object[] { pi.GetShortName(), typeof(FieldAttribute).GetShortName() });
        }
        if (pi.GetGetMethod(true) != null)
          collection.AddAspect(pi.GetGetMethod(true), new ImplementAutoPropertyReplacementAspect(persistentType, pi.Name));
        if (pi.GetSetMethod(true) != null)
          collection.AddAspect(pi.GetSetMethod(true), new ImplementAutoPropertyReplacementAspect(persistentType, pi.Name));
      }
    }

    private void ProvideConstructorAspect(Type type, LaosReflectionAspectCollection collection, Type baseType)
    {
      Type[] parameterTypes = type.IsSubclassOf(structureType)
                                ? new Type[] {persistentType, typeof (FieldInfo)}
                                : new[] {typeof (EntityData)}; 
      ImplementConstructorAspect ica = ImplementConstructorAspect.ApplyOnce(
        type,
        parameterTypes
        );
      collection.AddAspect(type, ica);
    }

    private Type ProvideConstructorDelegateAspect(Type type, LaosReflectionAspectCollection collection)
    {
      Type baseType = persistentType;

      ImplementConstructorAspect cda = null;
      if (type.IsSubclassOf(structureType)) {
        baseType = structureType;

        if (!type.IsAbstract)
          cda = ImplementConstructorAspect.ApplyOnce(
            type,
            new [] { persistentType, typeof(FieldInfo) });
      }

      if (type.IsSubclassOf(entityType)) {
        baseType = entityType;

        if (!type.IsAbstract)
          cda = ImplementConstructorAspect.ApplyOnce(
            type,
            new [] { typeof(EntityData) });
      }

      if (cda != null)
        collection.AddAspect(type, cda);
      return baseType;
    }


    // Constructors

    public StorageAttribute()
    {
      AttributeTargetTypes = "*";
    }
  }
}