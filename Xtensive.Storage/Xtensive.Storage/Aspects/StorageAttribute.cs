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
    private static readonly string handlerMethodSuffix = "Value";

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      Type type = element as Type;

      if (type == null)
        return;

      if (typeof(IContextBound<Session>).IsAssignableFrom(type) && type != typeof(Session))
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
      ProvideConstructorDelegateAspect(type, collection);

      ProvideConstructorAspect(type, collection);

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
          collection.AddAspect(pi.GetGetMethod(true), new ImplementAutoPropertyReplacementAspect(persistentType, handlerMethodSuffix));
        if (pi.GetSetMethod(true) != null)
          collection.AddAspect(pi.GetSetMethod(true), new ImplementAutoPropertyReplacementAspect(persistentType, handlerMethodSuffix));
      }
    }

    private void ProvideConstructorAspect(Type type, LaosReflectionAspectCollection collection)
    {
      Type[] parameterTypes = type.IsSubclassOf(structureType)
                                ? new Type[] {persistentType, typeof (FieldInfo)}
                                : new[] {typeof (EntityData)}; 
      ImplementConstructorAspect ica = ImplementConstructorAspect.ApplyOnce(
        type,
        parameterTypes
        );

      if (ica != null) {
        //Type baseType = type.BaseType;
        //while (null != baseType && null == baseType.GetConstructor(
        //  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
        //  null,
        //  parameterTypes,
        //  null)) {
        //  baseType = baseType.BaseType;
        //  ica.AspectPriority--;
        //}
        //AspectsMessageSource.Instance.Write(SeverityType.Debug, "AspectDebugMessage", new object[]
        //  { String.Format("\nConstructor for '{0}' has priority of '{1}'", type.FullName, ica.AspectPriority) } );
        collection.AddAspect(type, ica);
      }
    }

    private void ProvideConstructorDelegateAspect(Type type, LaosReflectionAspectCollection collection)
    {
      ImplementConstructorAspect cda = null;
      if (type.IsSubclassOf(structureType)) {
        if (!type.IsAbstract)
          cda = ImplementConstructorAspect.ApplyOnce(
            type,
            new [] { persistentType, typeof(FieldInfo) });
      }

      if (type.IsSubclassOf(entityType)) {
        if (!type.IsAbstract)
          cda = ImplementConstructorAspect.ApplyOnce(
            type,
            new [] { typeof(EntityData) });
      }

      if (cda != null)
        collection.AddAspect(type, cda);
    }


    // Constructors

    public StorageAttribute()
    {
      AttributeTargetTypes = "*";
    }
  }
}