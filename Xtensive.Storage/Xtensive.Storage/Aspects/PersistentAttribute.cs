// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Aspects;
using Xtensive.Storage.Resources;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Provides necessary aspects to <see cref="Persistent"/> and <see cref="SessionBound"/> descendants.
  /// </summary>
  /// <remarks>
  /// <list>
  ///   <listheader>PersistentAttribute applies following aspects on a target class:</listheader>
  ///   <item><see cref="TransactionalAspect"/> on all methods of <see cref="SessionBound"/></item>
  ///   <item><see cref="AutoPropertyReplacementAspect"/> on auto-properties with <see cref="FieldAttribute">[Field] attribute</see></item>
  ///   <item><see cref="ProtectedConstructorAspect"/> on <see cref="Persistent"/> and <see cref="EntitySet{TItem}"/> classes</item>
  ///   <item><see cref="ProtectedConstructorAccessorAspect"/> on <see cref="Persistent"/> and <see cref="EntitySet{TItem}"/> classes</item>
  /// </list>
  /// It is possible to apply <see cref="PersistentAttribute"/> to the whole assembly with persistent model, 
  /// in order to automatically apply this attribute to all types.
  /// </remarks>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [Serializable]
  public sealed class PersistentAttribute : CompoundAspect
  {
    private const string HandlerMethodSuffix = "FieldValue";
    private static readonly Type persistentType   = typeof(Persistent);
    private static readonly Type entityType       = typeof(Entity);
    private static readonly Type entitySetType    = typeof(EntitySetBase);
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
        ProvideTransactionalAspects(type, collection);
      if (persistentType.IsAssignableFrom(type))
        ProvidePersistentAspects(type, collection);
      if (entitySetType.IsAssignableFrom(type))
        ProvideEntitySetAspects(type, collection);
    }

    private static void ProvideTransactionalAspects(Type type, LaosReflectionAspectCollection collection)
    {
      // var explicitMethods = GetExplicitMethods(type).Cast<MethodBase>().ToHashSet();
      // ^^^ Does not work because of a bug in PostSharp. ^^^
      var explicitMethods = new HashSet<MethodBase>(); 
      var candidates = GetMethods(type).Cast<MethodBase>()
        .Concat(explicitMethods)
        // .Concat(GetPublicConstructors(type))
        .ToHashSet();

      foreach (MethodBase method in candidates) {
        if (method.IsAbstract)
            continue;
        if (method.IsStatic)
          continue;
        if (AspectHelper.IsInfrastructureMethod(method))
          continue;
        var behaviorAttribute = method.GetAttribute<AspectBehaviorAttribute>(AttributeSearchOptions.InheritNone);
        if (behaviorAttribute==null) {
          if (IsCompilerGenerated(method))
            continue;
          if (!method.IsPublic && !explicitMethods.Contains(method))
            continue; // Non-public and non-explicit implementation
          if (method.IsConstructor) {
            if (!entityType.IsAssignableFrom(type))
              continue;
            else {
              // Public constructors of Entities are transactional
              behaviorAttribute = new AspectBehaviorAttribute {
                OpenSession = false,
                OpenTransaction = true
              };
            }
          }
        }
        if (behaviorAttribute==null)
          behaviorAttribute = new AspectBehaviorAttribute();

        if (behaviorAttribute.OpenSession || behaviorAttribute.OpenTransaction) {
          var ta = TransactionalAspect.ApplyOnce(method,
            behaviorAttribute.OpenSession, behaviorAttribute.OpenTransaction);
          if (ta!=null)
            collection.AddAspect(method, ta);
        }
      }
    }

    private static void ProvidePersistentAspects(Type type, LaosReflectionAspectCollection collection)
    {
      ProvidePersistentFieldAspects(type, collection);
      ProvideInconsistencyRegionAspects(type, collection);
      ProvideConstructorAspect(type, collection);
      ProvideConstructorAccessorAspect(type, collection);
      new InitializableAttribute().ProvideAspects(type, collection);
    }

    private static void ProvideEntitySetAspects(Type type, LaosReflectionAspectCollection collection)
    {
      ProvideConstructorAccessorAspect(type, collection);
      ProvideConstructorAspect(type, collection);
//      new InitializableAttribute().ProvideAspects(type, collection);
    }

    private static void ProvidePersistentFieldAspects(Type type, LaosReflectionAspectCollection collection)
    {
      foreach (var propertyInfo in type.GetProperties(
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.DeclaredOnly)) 
      {
        var keyFieldAttribute = type.GetAttribute<KeyAttribute>(AttributeSearchOptions.InheritNone);
        try {
          var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(
            AttributeSearchOptions.InheritFromAllBase);
          if (fieldAttribute==null)
            continue;
        }
        catch (InvalidOperationException) {
          ErrorLog.Write(SeverityType.Error, AspectMessageType.AspectMustBeSingle,
            AspectHelper.FormatType(typeof(FieldAttribute)),
            AspectHelper.FormatMember(propertyInfo.DeclaringType, propertyInfo));
        }
        var getter = propertyInfo.GetGetMethod(true);
        var setter = propertyInfo.GetSetMethod(true);
        if (getter!=null) {
          var getterAspect = AutoPropertyReplacementAspect.ApplyOnce(getter, persistentType, HandlerMethodSuffix);
          if (getterAspect!=null)
            collection.AddAspect(getter, getterAspect);
        }
        if (setter!=null) {
          if (keyFieldAttribute!=null) {
            collection.AddAspect(setter, new NotSupportedMethodAspect(
                string.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, propertyInfo.Name, type.Name)));
            continue;
          }
          var setterAspect = AutoPropertyReplacementAspect.ApplyOnce(setter, persistentType, HandlerMethodSuffix);
          if (setterAspect!=null)
            collection.AddAspect(setter, setterAspect);

          // If there are constraints, we must "wrap" setter into transaction
          var constraints = propertyInfo.GetAttributes<PropertyConstraintAspect>(AttributeSearchOptions.InheritNone);
          bool hasConstraints = !(constraints==null || constraints.Length==0);
          if (hasConstraints) {
            var transactionalAspect = TransactionalAspect.ApplyOnce(setter, true, true);
            if (transactionalAspect!=null)
              collection.AddAspect(setter, transactionalAspect);
          }
        }
      }
    }

    private static void ProvideInconsistencyRegionAspects(Type type, LaosReflectionAspectCollection collection)
    {
      if (type==entityType || type==structureType || type==persistentType)
        return;
      foreach (var ctorInfo in type.GetConstructors(
        BindingFlags.Public |
        BindingFlags.Instance |
        BindingFlags.DeclaredOnly))
      {
        if (AspectHelper.IsInfrastructureMethod(ctorInfo))
          continue;
        if (IsCompilerGenerated(ctorInfo))
          continue;

        collection.AddAspect(ctorInfo, new InconsistentRegionAttribute());
      }
    }

    private static void ProvideConstructorAspect(Type type, LaosReflectionAspectCollection collection)
    {
      if (type==entityType || type==structureType || type==persistentType)
        return;
      var signatures = GetInternalConstructorSignatures(type)
        .Concat(new[] {new[] {typeof (SerializationInfo), typeof (StreamingContext)}});
      foreach (var signature in signatures) {
        var aspect = ProtectedConstructorAspect.ApplyOnce(type, signature);
        if (aspect != null && aspect.CompileTimeValidate(type))
          aspect.ProvideAspects(type, collection);
      }
    }

    private static void ProvideConstructorAccessorAspect(Type type, LaosReflectionAspectCollection collection)
    {
      if (type.IsAbstract)
        return;
      var signatures = GetInternalConstructorSignatures(type);
      foreach (var signature in signatures) {
        var aspect = ProtectedConstructorAccessorAspect.ApplyOnce(type, signature);
        if (aspect!=null)
          aspect.ProvideAspects(type, collection);
      }
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

    private static bool IsCompilerGenerated(MemberInfo member)
    {
      return 
        member.Name.StartsWith("<") || 
        null!=member.GetAttribute<CompilerGeneratedAttribute>(AttributeSearchOptions.InheritNone);
    }

    private static Type GetBasePersistentType(Type type)
    {
      if (structureType.IsAssignableFrom(type))
        return structureType;
      if (entityType.IsAssignableFrom(type))
        return entityType;
      if (entitySetType.IsAssignableFrom(type))
        return entitySetType;
      return null;
    }

    /// <exception cref="InvalidOperationException">[Suppresses warning]</exception>
    private static Type[][] GetInternalConstructorSignatures(Type type)
    {
      var baseType = GetBasePersistentType(type);
      if (baseType==structureType)
        return new[] {
          new[] {persistentType, typeof (FieldInfo)},
          new[] {typeof (Tuple)}
        };
      if (baseType==entityType)
        return new[] {
          new[] {typeof (EntityState)}
        };
      if (baseType==entitySetType)
        return new[] {
          new[] {entityType, typeof (FieldInfo)}
        };
      throw Exceptions.InternalError(
        string.Format(Strings.ExWrongPersistentTypeCandidate, type.GetType()), 
        Log.Instance);
    }

    private static IEnumerable<MethodInfo> GetMethods(Type type)
    {
      foreach (var method in type.GetMethods(
        BindingFlags.Public   | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.DeclaredOnly))
        yield return method;
    }

    private static IEnumerable<MethodInfo> GetExplicitMethods(Type type)
    {
      foreach (var @interface in type.FindInterfaces((_type, _criteris) => true, null)) {
        var map = type.GetInterfaceMap(@interface);
        foreach (var method in map.TargetMethods)
          if (method.IsPrivate)
            yield return method;
      }
    }

    private static IEnumerable<MethodBase> GetPublicConstructors(Type type)
    {
      foreach (var method in type.GetConstructors())
        yield return method;
    }

    #endregion
  }
}
