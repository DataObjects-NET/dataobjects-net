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
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Activator=System.Activator;

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
        var context = BuildingContext.Current;
        try {
          context.Definition = new DomainModelDef();
          DefineTypes();
        }
        catch (DomainBuilderException e) {
          context.RegisterError(e);
        }
        context.EnsureBuildSucceed();

        if (context.Configuration.Builders.Count>0)
          BuildCustomDefinitions();
      }
    }

    private static void DefineTypes()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogDefiningX, Strings.Types)) {
        var context = BuildingContext.Current;
        var typeFilter = context.BuilderConfiguration.TypeFilter ?? (t => true);
        foreach (var type in context.Configuration.Types)          
          if (typeFilter.Invoke(type))
            DefineType(type);
      }
    }

    private static void DefineType(Type type)
    {      
      var context = BuildingContext.Current;
      if (context.Definition.Types.Contains(type))
        return;
      try {
        var typeDef = TypeBuilder.DefineType(type);
        context.Definition.Types.Add(typeDef);
        IndexBuilder.DefineIndexes(typeDef);
      }
      catch (DomainBuilderException e) {
        context.RegisterError(e);
      }
    }

    private static void BuildCustomDefinitions()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        var context = BuildingContext.Current;
        foreach (var type in BuildingContext.Current.Configuration.Builders) {
          var builder = (IDomainBuilder) Activator.CreateInstance(type);
          builder.Build(context, context.Definition);
        }
      }
    }

    private static void BuildModel()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        var context = BuildingContext.Current;
        context.Model = new DomainModel();
        BuildTypes();
        context.ModelUnlockKey = context.Model.GetUnlockKey();
        context.Model.Lock(true);
      }
    }

    private static void BuildTypes()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        var context = BuildingContext.Current;

        foreach (var typeDef in context.Definition.Types.Where(t => !t.IsInterface)) {
          CheckPersistentAspect(typeDef);
          try {
            TypeBuilder.BuildType(typeDef);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
        }
        context.EnsureBuildSucceed();
        ValidateHierarchies();
        BuildAssociations();
        BuildColumns();
        IndexBuilder.BuildIndexes();
        BuildHierarchyColumns();
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void CheckPersistentAspect(TypeDef typeDef)
    {
      var constructor = typeDef.UnderlyingType.GetConstructor(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(EntityState), typeof(bool) });
      if (constructor == null && !typeDef.IsStructure) {
        var assemblyName = typeDef.UnderlyingType.Assembly.ManifestModule.Name;
        assemblyName = assemblyName.Remove(assemblyName.Length - 4);
        throw new DomainBuilderException(string.Format(
          Strings.ExPersistentAttributeIsNotSetOnTypeX, assemblyName));
      }
    }

    private static void ValidateHierarchies()
    {
      var context = BuildingContext.Current;
      foreach (var hierarchy in context.Definition.Hierarchies) {
        var root = hierarchy.Root;
        foreach (var keyField in hierarchy.KeyFields.Keys) {
          FieldDef srcField;
          if (!root.Fields.TryGetValue(keyField.Name, out srcField))
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
        foreach (var hierarchyInfo in BuildingContext.Current.Model.Hierarchies)
          HierarchyBuilder.BuildHierarchyColumns(hierarchyInfo);
      }
    }

    private static void BuildColumns()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Columns)) {
        foreach (var type in BuildingContext.Current.Model.Types) {
          type.Columns.Clear();
          type.Columns.AddRange(type.Fields.Where(f => f.Column!=null).Select(f => f.Column));
        }
      }
    }

    private static void BuildAssociations()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        var context = BuildingContext.Current;
        foreach (var pair in context.PairedAssociations) {
          if (context.DiscardedAssociations.Contains(pair.First))
            continue;
          try {
            AssociationBuilder.BuildPairedAssociation(pair.First, pair.Second);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
        }

        foreach (var ai in context.DiscardedAssociations)
          context.Model.Associations.Remove(ai);
        context.DiscardedAssociations.Clear();

        BuildEntitySetTypes(context.Model.Associations);
      }
    }

    private static void BuildEntitySetTypes(IEnumerable<AssociationInfo> associations)
    {
      var context = BuildingContext.Current;
      foreach (var association in associations) {
        if (!association.IsMaster)
          continue;

        var multiplicity = association.Multiplicity;
        if (!(multiplicity == Multiplicity.ZeroToMany || multiplicity == Multiplicity.ManyToMany))
          continue;

        var masterFieldType = association.ReferencedType;
        var slaveFieldType = association.ReferencingType;

        var underlyingGenericType = typeof (EntitySetItem<,>).MakeGenericType(masterFieldType.UnderlyingType, slaveFieldType.UnderlyingType);
        var underlyingType = TypeHelper.CreateDummyType(BuildingContext.Current.NameBuilder.Build(association), underlyingGenericType, true);

        var underlyingTypeDef = TypeBuilder.DefineType(underlyingType);
        underlyingTypeDef.Name = association.Name;

        var masterFieldDef = underlyingTypeDef.DefineField(underlyingType.GetProperty(context.NameBuilder.EntitySetItemMasterFieldName));
        var slaveFieldDef = underlyingTypeDef.DefineField(underlyingType.GetProperty(context.NameBuilder.EntitySetItemSlaveFieldName));

        if (masterFieldType!=slaveFieldType) {
          masterFieldDef.MappingName = context.NameBuilder.NamingConvention.Apply(masterFieldType.Name);
          slaveFieldDef.MappingName = context.NameBuilder.NamingConvention.Apply(slaveFieldType.Name);
        }
        context.Definition.Types.Add(underlyingTypeDef);
        IndexBuilder.DefineIndexes(underlyingTypeDef);

        var hierarchy = context.Definition.DefineHierarchy(underlyingTypeDef);
        hierarchy.KeyFields.Add(new KeyField(masterFieldDef.Name, masterFieldDef.ValueType), Direction.Positive);
        hierarchy.KeyFields.Add(new KeyField(slaveFieldDef.Name, slaveFieldDef.ValueType), Direction.Positive);

        TypeBuilder.BuildType(underlyingTypeDef);
        association.UnderlyingType = context.Model.Types[underlyingType];
      }
    }
  }
}