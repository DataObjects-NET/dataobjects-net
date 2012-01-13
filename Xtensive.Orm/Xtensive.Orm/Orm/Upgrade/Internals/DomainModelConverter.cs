// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Sorting;
using Xtensive.Modelling;
using Xtensive.Orm.Building;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Resources;
using ColumnInfo = Xtensive.Orm.Model.ColumnInfo;
using FullTextIndexInfo = Xtensive.Orm.Model.FullTextIndexInfo;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;
using IndexingModel = Xtensive.Orm.Upgrade.Model;
using ReferentialAction = Xtensive.Orm.Upgrade.Model.ReferentialAction;
using SequenceInfo = Xtensive.Orm.Upgrade.Model.SequenceInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Converts <see cref="DomainModel"/> to indexing storage model.
  /// </summary>
  internal sealed class DomainModelConverter : ModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the provider info.
    /// </summary>
    private readonly ProviderInfo providerInfo;

    /// <summary>
    /// Gets the storage info.
    /// </summary>
    private StorageInfo storageInfo;

    /// <summary>
    /// Gets the currently converting model.
    /// </summary>
    private DomainModel model;

    /// <summary>
    /// Gets a value indicating whether 
    /// build foreign keys for associations.
    /// </summary>
    private readonly bool buildForeignKeys;

    /// <summary>
    /// Gets or sets a value indicating whether 
    /// build foreign keys for hierarchies.
    /// </summary>
    private readonly bool buildHierarchyForeignKeys;

    /// <summary>
    /// Gets storage model builder.
    /// </summary>
    private readonly StorageModelBuilder storageModelBuilder;

    /// <summary>
    /// Gets or sets the currently visiting table.
    /// Gets name builder.
    /// </summary>
    private readonly NameBuilder nameBuilder;

    /// <summary>
    /// Gets or sets the currently visiting table.
    /// </summary>
    private TableInfo currentTable;

    /// <summary>
    /// Converts the specified <see cref="DomainModel"/> to
    /// <see cref="storageInfo"/>.
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <returns>The storage model.</returns>
    public IndexingModel.StorageInfo Convert(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");

      storageInfo = new IndexingModel.StorageInfo();
      model = domainModel;
      Visit(domainModel);
      return storageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode Visit(Orm.Model.Node node)
    {
      var indexInfo = node as Orm.Model.IndexInfo;
      if (indexInfo!=null && indexInfo.IsPrimary)
        return VisitPrimaryIndexInfo(indexInfo);

      return base.Visit(node);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDomainModel(DomainModel domainModel)
    {
      // Build tables, columns and primary indexes
      foreach (var primaryIndex in domainModel.RealIndexes.Where(i => i.IsPrimary))
        Visit(primaryIndex);

      // Build full-text indexes
      foreach (var fullTextIndex in domainModel.FullTextIndexes)
        Visit(fullTextIndex);

      // Build foreign keys
      if (buildForeignKeys && providerInfo.Supports(ProviderFeatures.ForeignKeyConstraints)) {
        foreach (var group in domainModel.Associations.Where(a => a.Ancestors.Count==0)) {
            Visit(group);
        }
      }

      // Build keys and sequences
      foreach (KeyInfo keyInfo in domainModel.Hierarchies.Select(h => h.Key))
        Visit(keyInfo);

      if (!buildHierarchyForeignKeys || !providerInfo.Supports(ProviderFeatures.ForeignKeyConstraints))
        return storageInfo;

      // Build hierarchy foreign keys
      var indexPairs = new Dictionary<Pair<IndexInfo>, object>();
      foreach (var type in domainModel.Types.Entities) {
        if (type.Hierarchy==null || type.Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable)
          continue;
        if (type.Indexes.PrimaryIndex.IsVirtual) {
          Dictionary<TypeInfo, int> typeOrder = type.GetAncestors()
            .AddOne(type)
            .Select((t, i) => new {Type = t, Index = i})
            .ToDictionary(a => a.Type, a => a.Index);
          List<IndexInfo> realPrimaryIndexes = type.Indexes.RealPrimaryIndexes
            .OrderBy(index => typeOrder[index.ReflectedType])
            .ToList();
          for (int i = 0; i < realPrimaryIndexes.Count - 1; i++) {
            if (realPrimaryIndexes[i]!=realPrimaryIndexes[i + 1]) {
              var pair = new Pair<IndexInfo>(realPrimaryIndexes[i], realPrimaryIndexes[i + 1]);
              indexPairs[pair] = null;
            }
          }
        }
      }
      foreach (var indexPair in indexPairs.Keys) {
        var referencedIndex = indexPair.First;
        var referencingIndex = indexPair.Second;
        var referencingTable = storageInfo.Tables[referencingIndex.ReflectedType.MappingName];
        var referencedTable = storageInfo.Tables[referencedIndex.ReflectedType.MappingName];
        var storageReferencingIndex = FindIndex(referencingTable,
          new List<string>(referencingIndex.KeyColumns.Select(ci => ci.Key.Name)));

        string foreignKeyName = nameBuilder.BuildHierarchyForeignKeyName(referencingIndex.ReflectedType, referencedIndex.ReflectedType);
        CreateHierarchyForeignKey(referencingTable, referencedTable, storageReferencingIndex, foreignKeyName);
      }

      return storageInfo;
    }

    /// <inheritdoc/>
    protected IPathNode VisitIndexInfo(IndexInfo primaryIndex, IndexInfo index)
    {
      IndexingModel.TableInfo table = currentTable;
      var secondaryIndex = storageModelBuilder.CreateSecondaryIndex(table, index.MappingName, index);
      secondaryIndex.IsUnique = index.IsUnique;
      var isClustered = index.IsClustered && providerInfo.Supports(ProviderFeatures.ClusteredIndexes);
      secondaryIndex.IsClustered = isClustered;
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns) {
        string columName = GetPrimaryIndexColumnName(primaryIndex, pair.Key, index);
        IndexingModel.ColumnInfo column = table.Columns[columName];
        new IndexingModel.KeyColumnRef(secondaryIndex, column,
          providerInfo.Supports(ProviderFeatures.KeyColumnSortOrder)
            ? pair.Value
            : Direction.Positive);
      }
      // At least SQL Server does not support clustered indexes with included columns.
      // For now this is the only RDBMS that have support for clustered indexes in DO.
      // Let's omit additional checks for ServerFeatures here
      // and simply ignore included columns for clustered indexes.
      if (providerInfo.Supports(ProviderFeatures.IncludedColumns) && !isClustered) {
        foreach (var includedColumn in index.IncludedColumns) {
          string columName = GetPrimaryIndexColumnName(primaryIndex, includedColumn, index);
          IndexingModel.ColumnInfo column = table.Columns[columName];
          new IndexingModel.IncludedColumnRef(secondaryIndex, column);
        }
      }
      secondaryIndex.PopulatePrimaryKeyColumns();
      return secondaryIndex;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitColumnInfo(ColumnInfo column)
    {
      var nonNullableType = column.ValueType;
      var nullableType    = ToNullable(nonNullableType, column.IsNullable);

      var typeInfoPrototype = new IndexingModel.TypeInfo(nullableType, column.IsNullable, 
        column.Length, column.Scale, column.Precision, null);
      var nativeTypeInfo = storageModelBuilder.CreateType(nonNullableType, column.Length, column.Precision, column.Scale);

      // We need the same type as in SQL database here (i.e. the same as native)
      var typeInfo = new IndexingModel.TypeInfo(ToNullable(nativeTypeInfo.Type, column.IsNullable), column.IsNullable, 
        nativeTypeInfo.Length, nativeTypeInfo.Scale, nativeTypeInfo.Precision, nativeTypeInfo.NativeType);

      var defaultValue = GetColumnDefaultValue(column, typeInfo);
      if (column.IsSystem && column.Field.IsTypeId) {
        var type = column.Field.ReflectedType;
        if (type.IsEntity && type==type.Hierarchy.Root) {
          var buildingContext = BuildingContext.Demand();
          defaultValue = buildingContext.BuilderConfiguration.TypeIdProvider(type.UnderlyingType);
        }
      }

      return new IndexingModel.ColumnInfo(currentTable, column.Name, typeInfo) {
        DefaultValue = defaultValue
      };
    }

    /// <inheritdoc/>
    protected override IPathNode VisitAssociationInfo(AssociationInfo association)
    {
      // Skip associations that do not impose constraints
      if (association.OnTargetRemove==OnRemoveAction.None)
        return null;

      if (association.AuxiliaryType==null && !association.OwnerField.IsEntitySet) {
        if (!IsValidForeignKeyTarget(association.TargetType))
          return null;
        if (association.OwnerType.IsInterface) {
          foreach (var implementorType in association.OwnerType.GetImplementors().SelectMany(GetForeignKeyOwners)) {
            var implementorField = implementorType.FieldMap[association.OwnerField];
            ProcessDirectAssociation(implementorType, implementorField, association.TargetType);
          }
        }
        else {
          foreach (var type in GetForeignKeyOwners(association.OwnerField.DeclaringType)) {
            ProcessDirectAssociation(type, association.OwnerField, association.TargetType);
          }
        }
      }
      else if (association.AuxiliaryType!=null && association.IsMaster) {
        ProcessIndirectAssociation(association.AuxiliaryType);
      }
      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitKeyInfo(KeyInfo keyInfo)
    {
      if (keyInfo.Sequence==null || !keyInfo.IsFirstAmongSimilarKeys)
        return null;
      var sequenceInfo = keyInfo.Sequence;
      long increment = 1;
      if (providerInfo.Supports(ProviderFeatures.ArbitraryIdentityIncrement) || providerInfo.Supports(ProviderFeatures.Sequences))
        increment = sequenceInfo.Increment;
      var sequence = new IndexingModel.SequenceInfo(storageInfo, sequenceInfo.MappingName) {
        Seed = sequenceInfo.Seed,
        Increment = increment,
        Type = storageModelBuilder.CreateType(keyInfo.TupleDescriptor[0], null, null, null),
      };
      return sequence;
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Thrown always by this method.</exception>
    protected override IPathNode VisitSequenceInfo(Orm.Model.SequenceInfo info)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitFullTextIndexInfo(FullTextIndexInfo fullTextIndex)
    {
      var table = GetTable(fullTextIndex.PrimaryIndex.ReflectedType);
      var primaryIndex = table.PrimaryIndex;
      var ftIndex = new IndexingModel.FullTextIndexInfo(table, fullTextIndex.Name);
      foreach (var fullTextColumn in fullTextIndex.Columns) {
        var column = table.Columns[fullTextColumn.Name];
        var typeColumn = fullTextColumn.TypeColumn == null
          ? null
          : primaryIndex.ValueColumns[fullTextColumn.TypeColumn.Name];
        var ftColumn = new IndexingModel.FullTextColumnRef(ftIndex, column, fullTextColumn.Configuration, typeColumn);
      }
      return ftIndex;
    }

    /// <summary>
    /// Visits primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    private IPathNode VisitPrimaryIndexInfo(IndexInfo index)
    {
      var tableName = index.ReflectedType.MappingName;
      currentTable = new IndexingModel.TableInfo(storageInfo, tableName);
      foreach (var column in index.Columns)
        Visit(column);

      // Support for mysql as primary indexes there always have name 'PRIMARY'
      string name = providerInfo.ConstantPrimaryIndexName;
      if (string.IsNullOrEmpty(name))
        name = index.MappingName;

      var primaryIndex = new IndexingModel.PrimaryIndexInfo(currentTable, name);
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns) {
        string columName = GetPrimaryIndexColumnName(index, pair.Key, index);
        var column = currentTable.Columns[columName];
        new IndexingModel.KeyColumnRef(primaryIndex, column,
          providerInfo.Supports(ProviderFeatures.KeyColumnSortOrder)
            ? pair.Value
            : Direction.Positive);
      }
      primaryIndex.PopulateValueColumns();
      primaryIndex.IsClustered = index.IsClustered && providerInfo.Supports(ProviderFeatures.ClusteredIndexes);

      foreach (var secondaryIndex in index.ReflectedType.Indexes.Where(i => i.IsSecondary && !i.IsVirtual))
        VisitIndexInfo(index, secondaryIndex);

      currentTable = null;
      return primaryIndex;
    }

    #region Not supported

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitKeyField(KeyField keyField)
    {
      throw new NotSupportedException(String.Format(Strings.ExVisitKeyFieldIsNotSupportedByX, typeof (DomainModelConverter)));
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitFieldInfo(FieldInfo field)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitHierarchyInfo(HierarchyInfo hierarchy)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTypeInfo(TypeInfo type)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitIndexInfo(IndexInfo index)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region Helper methods

    private object GetColumnDefaultValue(ColumnInfo column, IndexingModel.TypeInfo typeInfo)
    {
      if (column.DefaultValue!=null)
        return column.DefaultValue;

      if (column.IsNullable)
        return null;
      
      var type = typeInfo.Type;
      if (type==typeof(string))
        return string.Empty;
      if (type==typeof(byte[]))
        return ArrayUtils<byte>.EmptyArray;
      return Activator.CreateInstance(column.ValueType);
    }

    private static ReferentialAction ConvertReferentialAction(OnRemoveAction source)
    {
      switch (source) {
      case OnRemoveAction.Deny:
        return ReferentialAction.Restrict;
      case OnRemoveAction.Cascade:
        return ReferentialAction.Cascade;
      case OnRemoveAction.Clear:
        return ReferentialAction.Clear;
      default:
        return ReferentialAction.Default;
      }
    }

    private static IndexingModel.IndexInfo FindIndex(TableInfo table, IEnumerable<string> keyColumns)
    {
      IEnumerable<string> primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value.Name);
      if (primaryKeyColumns.Except(keyColumns)
        .Union(keyColumns.Except(primaryKeyColumns)).Count()==0)
        return table.PrimaryIndex;

      foreach (SecondaryIndexInfo index in table.SecondaryIndexes) {
        IEnumerable<string> secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value.Name);
        if (secondaryKeyColumns.Except(keyColumns)
          .Union(keyColumns.Except(secondaryKeyColumns)).Count()==0)
          return index;
      }
      return null;
    }

    private static IndexInfo FindIndex(IndexInfo index, FieldInfo field)
    {
      if (index.IsVirtual) {
        foreach (var underlyingIndex in index.UnderlyingIndexes) {
          var result = FindIndex(underlyingIndex, field);
          if (result!=null)
            return result;
        }
      }
      else if (field==null || index.Columns.ContainsAny(field.Columns))
        return index;
      return null;
    }

    private static IndexInfo FindNonVirtualPrimaryIndex(IndexInfo index)
    {
      if (index.IsPrimary && !index.IsVirtual)
        return index;
      var primaryIndex = index.ReflectedType.Indexes.PrimaryIndex;

      return
        primaryIndex.IsVirtual
          ? primaryIndex.DeclaringIndex
          : primaryIndex;
    }

    private TableInfo GetTable(TypeInfo type)
    {
      if (type.Hierarchy==null || type.Hierarchy.InheritanceSchema!=InheritanceSchema.SingleTable)
        return storageInfo.Tables.FirstOrDefault(table => table.Name==type.MappingName);
      if (type.IsInterface)
        return null;
      return storageInfo.Tables.FirstOrDefault(table => table.Name==type.Hierarchy.Root.MappingName);
    }

    private static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, ColumnInfo secondaryIndexColumn, IndexInfo secondaryIndex)
    {
      string primaryIndexColumnName = null;
      foreach (var primaryColumn in primaryIndex.Columns)
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field)) {
          primaryIndexColumnName = primaryColumn.Name;
          break;
        }
      return primaryIndexColumnName;
    }

    private static void CreateReferenceForeignKey(TableInfo referencingTable, TableInfo referencedTable, FieldInfo referencingField, string foreignKeyName)
    {
      var foreignColumns = referencingField.Columns.Select(column => referencingTable.Columns[column.Name]).ToList();
      var foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName)
      {
        PrimaryKey = referencedTable.PrimaryIndex,
        OnRemoveAction = ReferentialAction.None,
        OnUpdateAction = ReferentialAction.None
      };
      foreach (var foreignColumn in foreignColumns)
        new ForeignKeyColumnRef(foreignKey, foreignColumn);
    }

    private static void CreateHierarchyForeignKey(TableInfo referencingTable, TableInfo referencedTable, IndexingModel.IndexInfo referencingIndex, string foreignKeyName)
    {
      var foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName)
      {
        PrimaryKey = referencedTable.PrimaryIndex,
        OnRemoveAction = ReferentialAction.None,
        OnUpdateAction = ReferentialAction.None
      };
      foreignKey.ForeignKeyColumns.Set(referencingIndex);
    }

    private static Type ToNullable(Type type, bool isNullable)
    {
      return isNullable && type.IsValueType && !type.IsNullable()
        ? type.ToNullable()
        : type;
    }

    private void ProcessDirectAssociation(TypeInfo ownerType, FieldInfo ownerField, TypeInfo targetType)
    {
      var referencingTable = GetTable(ownerType);
      var referencedTable = GetTable(targetType);
      if (referencedTable == null || referencingTable == null)
        return;
      var foreignKeyName = nameBuilder.BuildReferenceForeignKeyName(ownerType, ownerField, targetType);
      CreateReferenceForeignKey(referencingTable, referencedTable, ownerField, foreignKeyName);
    }

    private void ProcessIndirectAssociation(TypeInfo auxiliaryType)
    {
      var referencingTable = GetTable(auxiliaryType);
      if (referencingTable == null)
        return;
      foreach (var field in auxiliaryType.Fields.Where(fieldInfo => fieldInfo.IsEntity))
      {
        var referencedType = model.Types[field.ValueType];
        if (!IsValidForeignKeyTarget(referencedType))
          continue;
        var referencedTable = GetTable(referencedType);
        if (referencedTable == null)
          continue;
        var foreignKeyName = nameBuilder.BuildReferenceForeignKeyName(auxiliaryType, field, referencedType);
        CreateReferenceForeignKey(referencingTable, referencedTable, field, foreignKeyName);
      }
    }

    private IEnumerable<TypeInfo> GetForeignKeyOwners(TypeInfo type)
    {
      if (type.Hierarchy == null)
        yield break;
      yield return type;
      if (type.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable)
        foreach (var descendant in type.GetDescendants(true).Where(descendant => descendant.Indexes.PrimaryIndex != null))
          yield return descendant;
    }

    private static bool IsValidForeignKeyTarget(TypeInfo targetType)
    {
      return targetType.Hierarchy != null && (targetType.Hierarchy.InheritanceSchema != InheritanceSchema.ConcreteTable || targetType.IsLeaf);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="providerInfo">Information about underlying storage.</param>
    /// <param name="storageModelBuilder">Storage model builder.</param>
    /// <param name="buildForeignKeys">If set to <see langword="true"/>, foreign keys
    /// will be created for associations.</param>
    /// <param name="buildHierarchyForeignKeys">If set to <see langword="true"/>, foreign keys
    /// will be created for hierarchies.</param>
    public DomainModelConverter(
      ProviderInfo providerInfo, 
      StorageModelBuilder storageModelBuilder,
      NameBuilder nameBuilder,
      bool buildForeignKeys,
      bool buildHierarchyForeignKeys)
    {
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      ArgumentValidator.EnsureArgumentNotNull(storageModelBuilder, "storageModelBuilder");
      ArgumentValidator.EnsureArgumentNotNull(nameBuilder, "nameBuilder");

      this.providerInfo = providerInfo;
      this.storageModelBuilder = storageModelBuilder;
      this.nameBuilder = nameBuilder;
      this.buildForeignKeys = buildForeignKeys;
      this.buildHierarchyForeignKeys = buildHierarchyForeignKeys;
    }
  }
}