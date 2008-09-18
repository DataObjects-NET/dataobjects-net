// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Relations;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ModelBuilder
  {
    public static void Build()
    {
      BuildDefinition();
      BuildModel();
    }

    private static void BuildDefinition()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.ModelDefinition)) {
        BuildingContext context = BuildingContext.Current;
        try {
          context.Definition = new DomainModelDef();

          DefineTypes();
          DefineServices();
        }
        catch (DomainBuilderException e) {
          context.RegisterError(e);
        }
        context.EnsureBuildSucceed();

        if (context.Configuration.Builders.Count==0)
          return;

        BuildCustomDefinitions();
      }
    }

    private static void BuildCustomDefinitions()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        BuildingContext context = BuildingContext.Current;
        foreach (Type type in BuildingContext.Current.Configuration.Builders) {
          var builder = (IDomainBuilder) Activator.CreateInstance(type);
          builder.Build(context, context.Definition);
        }
      }
    }

    private static void BuildModel()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        BuildingContext context = BuildingContext.Current;
        context.Model = new DomainModel();
        BuildTypes();
        context.Model.Lock(true);
      }
    }

    private static void DefineTypes()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogDefiningX, Strings.Types)) {
        BuildingContext context = BuildingContext.Current;
        foreach (Type type in context.Configuration.Types) {
          try {
            TypeDef typeDef = TypeBuilder.DefineType(type);
            context.Definition.Types.Add(typeDef);
            IndexBuilder.DefineIndexes(typeDef);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
        }
      }
    }

    private static void DefineServices()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Services)) {
      }
    }

    private static void BuildTypes()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        BuildingContext context = BuildingContext.Current;

        foreach (TypeDef typeDef in context.Definition.Types.Where(t => !t.IsInterface))
          try {
            TypeBuilder.BuildType(typeDef);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }

        ValidateHierarchies();
        BuildAssociations();
        BuildColumns();
        IndexBuilder.BuildIndexes();
        BuildHierarchyColumns();
      }
    }

    private static void ValidateHierarchies()
    {
      BuildingContext context = BuildingContext.Current;
      foreach (HierarchyDef hierarchy in context.Definition.Hierarchies) {
        TypeDef root = hierarchy.Root;
        foreach (KeyField keyField in hierarchy.KeyFields.Keys) {
          FieldDef srcField = root.Fields.TryGetValue(keyField.Name);
          if (srcField == null)
            context.RegisterError(new DomainBuilderException(
              string.Format(Strings.ExKeyFieldXWasNotFoundInTypeY, keyField.Name, root.Name)));
          else if (srcField.ValueType!=keyField.ValueType)
            context.RegisterError(new DomainBuilderException(
              string.Format(Strings.ValueTypeMismatchForFieldX, keyField.Name)));
          else if (srcField.UnderlyingProperty != null) {
            var setMethod = srcField.UnderlyingProperty.GetSetMethod(true);
            if (setMethod != null) {
              if ((setMethod.Attributes & MethodAttributes.Private) == 0)
                context.RegisterError(new DomainBuilderException(
                  string.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, keyField.Name, root.Name)));
            }
          }
        }
      }
    }

    private static void BuildHierarchyColumns()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.HierarchyColumns)) {
        foreach (HierarchyInfo hierarchyInfo in BuildingContext.Current.Model.Hierarchies)
          HierarchyBuilder.BuildHierarchyColumns(hierarchyInfo);
      }
    }

    private static void BuildColumns()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Columns)) {
        foreach (TypeInfo type in BuildingContext.Current.Model.Types) {
          type.Columns.Clear();
          type.Columns.AddRange(type.Fields.Where(f => f.Column!=null).Select(f => f.Column));
        }
      }
    }

    private static void BuildAssociations()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        BuildingContext context = BuildingContext.Current;
        foreach (Pair<AssociationInfo, string> pair in context.PairedAssociations) {
          if (context.DiscardedAssociations.Contains(pair.First))
            continue;
          try {
            AssociationBuilder.BuildPairedAssociation(pair.First, pair.Second);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
        }

        foreach (AssociationInfo ai in context.DiscardedAssociations)
          context.Model.Associations.Remove(ai);
        context.DiscardedAssociations.Clear();

        BuildReferenceTypes(context.Model.Associations);
        BuildRelationManagers(context.Model.Associations);
      }
    }

    private static void BuildRelationManagers(IEnumerable<AssociationInfo> associations)
    {
      foreach (AssociationInfo association in associations) {
        if (association.Multiplicity==Multiplicity.OneToOne
          && association.ReferencingField.UnderlyingProperty!=null
            && association.PairTo.ReferencingField.UnderlyingProperty!=null
              && association.IsMaster) {
          Type variatorType = TypeHelper.CreateDummyType("RelationManagerVariator", typeof (object));
          Type firstType = association.ReferencingType.UnderlyingType;
          Type secondType = association.ReferencedType.UnderlyingType;
          Type managerType = typeof (OneToOneRelationManager<,,>).MakeGenericType(firstType, secondType, variatorType);
          string masterPropertyName = association.MasterAssociation.ReferencingField.UnderlyingProperty.Name;
          string slavePropertyName = association.MasterAssociation.PairTo.ReferencingField.UnderlyingProperty.Name;
          managerType.InvokeMember("Initialize",
            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
            null,
            null,
            new object[] {masterPropertyName, slavePropertyName});
          association.MasterAssociation.SetMaster = GetDelegate(managerType, "SetMaster", firstType, secondType);
          association.MasterAssociation.SetSlave  = GetDelegate(managerType, "SetSlave", secondType, firstType);
        }
      }
    }

    private static Delegate GetDelegate(Type type, string name, Type firstType, Type secondType)
    {
      MethodInfo methodInfo = typeof(ModelBuilder).GetMethod("GetRelationManagerDelegate", BindingFlags.Static | BindingFlags.NonPublic);
      var genericMethod = methodInfo.MakeGenericMethod(firstType, secondType);
      return (Delegate)genericMethod.Invoke(null, new object[] { type, name });
    }

// ReSharper disable UnusedPrivateMember
    private static Action<Entity, TSecond, Model.FieldInfo, Action<Model.FieldInfo, TSecond>> GetRelationManagerDelegate<TFirst, TSecond>(Type managerType, string name) 
      where TFirst:Entity 
      where TSecond : Entity
    {
      var action = DelegateHelper.CreateDelegate<Action<TFirst, TSecond, Action<TFirst, TSecond>>>(null, managerType, name, ArrayUtils<Type>.EmptyArray);
      return (a, b, c, d) => action((TFirst) a, b, ((first, second) => d(c, b)));
    }
// ReSharper restore UnusedPrivateMember

    private static void BuildReferenceTypes(IEnumerable<AssociationInfo> associations)
    {
      foreach (AssociationInfo association in associations) {
        association.EntityType = BuildReferenceType(association);
        if (association.EntityType!=null) {
          TypeDef typeDef = TypeBuilder.DefineType(association.EntityType);
          typeDef.DefineField("Entity1", association.ReferencedType.UnderlyingType);
          typeDef.DefineField("Entity2", association.ReferencingType.UnderlyingType);
          typeDef.Name = association.Name;
          BuildingContext.Current.Definition.Types.Add(typeDef);
          IndexBuilder.DefineIndexes(typeDef);
          TypeBuilder.BuildType(typeDef);
        }
      }
    }

    private static Type BuildReferenceType(AssociationInfo association)
    {
      if (association.ReferencingField.IsEntitySet && association.IsMaster) {
        Type baseType = typeof (EntitySetReference<,>).MakeGenericType(association.ReferencedType.UnderlyingType, association.ReferencingType.UnderlyingType);
        string name = BuildingContext.Current.NameBuilder.Build(association);
        return TypeHelper.CreateDummyType(name, baseType, true);
      }
      return null;
    }
  }
}