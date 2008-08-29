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
using Xtensive.Storage.Resources;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Must be applied to any <see cref="Persistent"/> and <see cref="SessionBound"/> type. 
  /// Normally - by applying (multicasting) it to the whole assembly.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [Serializable]
  public sealed class PersistentAttribute : CompoundAspect
  {
    private const string HandlerMethodSuffix = "Value";
    private static readonly Type persistentType   = typeof(Persistent);
    private static readonly Type entityType       = typeof(Entity);
    private static readonly Type structureType    = typeof(Structure);
    private static readonly Type sessionBoundType = typeof(SessionBound);

    /// <inheritdoc/>
    public override bool CompileTimeValidate(object element)
    {
      var type = element as Type;
      if (type == null)
        return false;

      if (!persistentType.IsAssignableFrom(type) && !sessionBoundType.IsAssignableFrom(type))
        return false;

      return true;
    }

    #region ProvideXxx methods

    /// <inheritdoc/>
    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      var type = (Type) element;
      if (sessionBoundType.IsAssignableFrom(type))
        ProvideSessionBoundAspects(type, collection);
      if (persistentType.IsAssignableFrom(type))
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
        if (mi.IsAbstract)
          continue;
        collection.AddAspect(mi, new SessionBoundMethodAspect());
      }
    }

    private void ProvidePersistentAspects(Type type, LaosReflectionAspectCollection collection)
    {
      ProvideConstructorDelegateAspect(type, collection);
      ProvideAutoPropertyAspects(type, collection);
      ProvideConstructorAspect(type, collection);
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
          var fieldAttribute = pi.GetAttribute<FieldAttribute>(
            AttributeSearchOptions.InheritFromAllBase);
          if (fieldAttribute==null)
            continue;
        }
        catch (InvalidOperationException) {
          ErrorLog.Write(SeverityType.Error, AspectMessageType.AspectMustBeSingle,
            AspectHelper.FormatType(typeof(FieldAttribute)),
            AspectHelper.FormatMember(pi.DeclaringType, pi));
        }
        var getter = pi.GetGetMethod(true);
        var setter = pi.GetSetMethod(true);
        if (getter!=null) {
          var getterAspect = ImplementAutoPropertyReplacementAspect.ApplyOnce(getter, persistentType, HandlerMethodSuffix);
          if (getterAspect!=null)
            collection.AddAspect(getter, getterAspect);
        }
        if (setter!=null) {
          var setterAspect = ImplementAutoPropertyReplacementAspect.ApplyOnce(setter, persistentType, HandlerMethodSuffix);
          if (setterAspect!=null)
            collection.AddAspect(setter, setterAspect);
        }
      }
    }

    private void ProvideConstructorAspect(Type type, LaosReflectionAspectCollection collection)
    {
      if (type==entityType || type==structureType || type==persistentType)
        return;
      var aspect = ImplementConstructorAspect.ApplyOnce(type, 
        GetInternalConstructorParameterTypes(type));
      if (aspect!=null)
        collection.AddAspect(type, aspect);
    }

    private void ProvideConstructorDelegateAspect(Type type, LaosReflectionAspectCollection collection)
    {
      if (type.IsAbstract)
        return;
      var aspect = ImplementProtectedConstructorAccessorAspect.ApplyOnce(type,
        GetInternalConstructorParameterTypes(type));
      if (aspect!=null)
        collection.AddAspect(type, aspect);
    }

    #endregion

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      var requirements = base.GetPostSharpRequirements();
      AspectHelper.AddStandardRequirements(requirements);
      return requirements;
    }

    #region Private \ internal methods

    private Type GetBasePersistentType(Type type)
    {
      if (structureType.IsAssignableFrom(type))
        return structureType;
      if (entityType.IsAssignableFrom(type))
        return entityType;
      return null;
    }

    /// <exception cref="Exception">[Suppresses warning]</exception>
    private Type[] GetInternalConstructorParameterTypes(Type type)
    {
      var baseType = GetBasePersistentType(type);
      if (baseType==structureType)
        return new[] {persistentType, typeof (FieldInfo)};
      if (baseType==entityType)
        return new[] {typeof (EntityData)};
      throw Exceptions.InternalError(
        string.Format(Strings.ExWrongPersistentTypeCandidate, type.GetType()), 
        Log.Instance);
    }

    #endregion
  }
}
