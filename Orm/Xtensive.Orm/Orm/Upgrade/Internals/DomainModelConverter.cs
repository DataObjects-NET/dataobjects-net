// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using PartialIndexFilterInfo = Xtensive.Orm.Upgrade.Model.PartialIndexFilterInfo;
using ReferentialAction = Xtensive.Orm.Upgrade.Model.ReferentialAction;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Converts <see cref="DomainModel"/> to indexing storage model.
  /// </summary>
  internal sealed class DomainModelConverter : ModelVisitor<IPathNode>
  {
    private static readonly string MetadataNamespace = typeof (Metadata.MetadataBase).Namespace;

    private readonly HandlerAccessor handlers;

    private readonly ProviderInfo providerInfo;
    private readonly DomainModel sourceModel;
    private readonly NameBuilder nameBuilder;
    private readonly StorageDriver driver;
    private readonly MappingResolver resolver;
    private readonly ITypeIdProvider typeIdProvider;
    private readonly DomainConfiguration configuration;
    private readonly PartialIndexFilterCompiler compiler;

    private StorageModel targetModel;
    private TableInfo currentTable;

    public bool BuildForeignKeys { get; set; }
    public bool BuildHierarchyForeignKeys { get; set; }

    /// <summary>
    /// Converts the specified <see cref="DomainModel"/> to
    /// <see cref="targetModel"/>.
    /// </summary>
    public StorageModel Run()
    {
      if (targetModel==null) {
        targetModel = new StorageModel();
        Visit(sourceModel);
      }

      return targetModel;
    }

    /// <inheritdoc/>
    protected override IPathNode Visit(Orm.Model.Node node)
    {
      var indexInfo = node as IndexInfo;
      if (indexInfo!=null && indexInfo.IsPrimary) {
        foreach (var table in CreateTables(indexInfo)) {
          currentTable = table;
          var result = VisitPrimaryIndexInfo(indexInfo);
          currentTable = null;
        }
        return null;
      }

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
      var buildForeignKeys = BuildForeignKeys
        && providerInfo.Supports(ProviderFeatures.ForeignKeyConstraints);

      if (buildForeignKeys)
        foreach (var group in domainModel.Associations.Where(a => a.Ancestors.Count==0))
          Visit(group);

      // Build keys and sequences
      foreach (KeyInfo keyInfo in domainModel.Hierarchies.Select(h => h.Key))
        Visit(keyInfo);

      var buildHierarchyForeignKeys = BuildHierarchyForeignKeys
        && providerInfo.Supports(ProviderFeatures.ForeignKeyConstraints);

      if (!buildHierarchyForeignKeys)
        return targetModel;

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
        var referencingTable = targetModel.Tables[resolver.GetNodeName(referencingIndex.ReflectedType)];
        var referencedTable = targetModel.Tables[resolver.GetNodeName(referencedIndex.ReflectedType)];
        var storageReferencingIndex = FindIndex(
          referencingTable, referencingIndex.KeyColumns.Select(ci => ci.Key.Name).ToList());

        string foreignKeyName = nameBuilder.BuildHierarchyForeignKeyName(referencingIndex.ReflectedType, referencedIndex.ReflectedType);
        CreateHierarchyForeignKey(referencingTable, referencedTable, storageReferencingIndex, foreignKeyName);
      }

      return targetModel;
    }

    private void VisitIndexInfo(IndexInfo primaryIndex, IndexInfo index)
    {
      TableInfo table = currentTable;
      var secondaryIndex = CreateSecondaryIndex(table, index.MappingName, index);
      secondaryIndex.IsUnique = index.IsUnique;
      var isClustered = index.IsClustered && providerInfo.Supports(ProviderFeatures.ClusteredIndexes);
      secondaryIndex.IsClustered = isClustered;
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns) {
        string columName = GetPrimaryIndexColumnName(primaryIndex, pair.Key, index);
        StorageColumnInfo column = table.Columns[columName];
        new KeyColumnRef(secondaryIndex, column,
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
          StorageColumnInfo column = table.Columns[columName];
          new IncludedColumnRef(secondaryIndex, column);
        }
      }
      secondaryIndex.PopulatePrimaryKeyColumns();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitColumnInfo(ColumnInfo column)
    {
      var nonNullableType = column.ValueType;
      var nullableType    = ToNullable(nonNullableType, column.IsNullable);

      var typeInfoPrototype = new StorageTypeInfo(nullableType, null, column.IsNullable, column.Length, column.Precision, column.Scale);
      var nativeTypeInfo = CreateType(nonNullableType, column.Length, column.Precision, column.Scale);

      // We need the same type as in SQL database here (i.e. the same as native)
      var typeInfo = new StorageTypeInfo(ToNullable(nativeTypeInfo.Type, column.IsNullable), nativeTypeInfo.NativeType, column.IsNullable, nativeTypeInfo.Length, nativeTypeInfo.Precision, nativeTypeInfo.Scale);

      var defaultValue = GetColumnDefaultValue(column, typeInfo);
      if (column.IsSystem && column.Field.IsTypeId) {
        var type = column.Field.ReflectedType;
        if (type.IsEntity && type==type.Hierarchy.Root) {
          defaultValue = typeIdProvider.GetTypeId(type.UnderlyingType);
        }
      }

      return new StorageColumnInfo(currentTable, column.Name, typeInfo) {
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
      var sequence = new StorageSequenceInfo(targetModel, resolver.GetNodeName(sequenceInfo)) {
        Seed = sequenceInfo.Seed,
        Increment = increment,
        Type = CreateType(keyInfo.TupleDescriptor[0], null, null, null),
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
      if (!providerInfo.Supports(ProviderFeatures.FullText)) {
        UpgradeLog.Warning(Strings.LogFullTextIndexesAreNotSupportedByCurrentStorageIgnoringIndexX, fullTextIndex.Name);
        return null;
      }

      var table = GetTable(fullTextIndex.PrimaryIndex.ReflectedType);
      var primaryIndex = table.PrimaryIndex;
      var ftIndex = new StorageFullTextIndexInfo(table, fullTextIndex.Name);
      foreach (var fullTextColumn in fullTextIndex.Columns) {
        var column = table.Columns[fullTextColumn.Name];
        string typeColumn = null;
        if(providerInfo.Supports(ProviderFeatures.FullTextColumnDataTypeSpecification)) {
          if (fullTextColumn.TypeColumn!=null)
            typeColumn = table.Columns[fullTextColumn.TypeColumn.Name].Name;
        }
        else
          UpgradeLog.Warning(Strings.LogSpecificationOfTypeColumnForFulltextColumnIsNotSupportedByCurrentStorageIgnoringTypeColumnSpecificationForColumnX, fullTextColumn.Column.Name);
        new FullTextColumnRef(ftIndex, column, fullTextColumn.Configuration, typeColumn);
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
      foreach (var column in index.Columns)
        Visit(column);

      // Support for mysql as primary indexes there always have name 'PRIMARY'
      string name = providerInfo.ConstantPrimaryIndexName;
      if (string.IsNullOrEmpty(name))
        name = index.MappingName;

      var primaryIndex = new PrimaryIndexInfo(currentTable, name);
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns) {
        string columName = GetPrimaryIndexColumnName(index, pair.Key, index);
        var column = currentTable.Columns[columName];
        new KeyColumnRef(primaryIndex, column,
          providerInfo.Supports(ProviderFeatures.KeyColumnSortOrder)
            ? pair.Value
            : Direction.Positive);
      }
      primaryIndex.PopulateValueColumns();
      primaryIndex.IsClustered = index.IsClustered && providerInfo.Supports(ProviderFeatures.ClusteredIndexes);

      foreach (var secondaryIndex in index.ReflectedType.Indexes.Where(i => i.IsSecondary && !i.IsVirtual))
        VisitIndexInfo(index, secondaryIndex);
      return primaryIndex;
    }

    private IEnumerable<TableInfo> CreateTables(IndexInfo index)
    {
      var result = new List<TableInfo>();
      var type = index.ReflectedType;
      if (configuration.IsMultidatabase && type.UnderlyingType.Namespace==MetadataNamespace) {
        foreach (var db in sourceModel.Databases) {
          var name = resolver.GetNodeName(db.Name, type.MappingSchema, type.MappingName);
          result.Add(new TableInfo(targetModel, name));
        }
      }
      else
        result.Add(new TableInfo(targetModel, resolver.GetNodeName(type)));
      return result;
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

    private object GetColumnDefaultValue(ColumnInfo column, StorageTypeInfo typeInfo)
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

    private static StorageIndexInfo FindIndex(TableInfo table, ICollection<string> keyColumns)
    {
      var primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value.Name).ToList();

      if (!primaryKeyColumns.Except(keyColumns).Union(keyColumns.Except(primaryKeyColumns)).Any())
        return table.PrimaryIndex;

      foreach (SecondaryIndexInfo index in table.SecondaryIndexes) {
        var secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value.Name).ToList();
        if (!secondaryKeyColumns.Except(keyColumns).Union(keyColumns.Except(secondaryKeyColumns)).Any())
          return index;
      }
      return null;
    }

    private TableInfo GetTable(TypeInfo type)
    {
      if (type.Hierarchy==null || type.Hierarchy.InheritanceSchema!=InheritanceSchema.SingleTable) {
        var name = resolver.GetNodeName(type);
        return targetModel.Tables.FirstOrDefault(t => t.Name==name);
      }
      if (!type.IsInterface) {
        var name = resolver.GetNodeName(type.Hierarchy.Root);
        return targetModel.Tables.FirstOrDefault(table => table.Name==name);
      }
      return null;
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
      var foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName) {
        PrimaryKey = referencedTable.PrimaryIndex,
        OnRemoveAction = ReferentialAction.None,
        OnUpdateAction = ReferentialAction.None
      };
      foreach (var foreignColumn in foreignColumns)
        new ForeignKeyColumnRef(foreignKey, foreignColumn);
    }

    private static void CreateHierarchyForeignKey(TableInfo referencingTable, TableInfo referencedTable, StorageIndexInfo referencingIndex, string foreignKeyName)
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
      if (!AreMappedToSameDatabase(ownerType, targetType))
        return;
      var referencingTable = GetTable(ownerType);
      var referencedTable = GetTable(targetType);
      if (referencedTable==null || referencingTable==null)
        return;
      var foreignKeyName = nameBuilder.BuildReferenceForeignKeyName(ownerType, ownerField, targetType);
      CreateReferenceForeignKey(referencingTable, referencedTable, ownerField, foreignKeyName);
    }

    private void ProcessIndirectAssociation(TypeInfo auxiliaryType)
    {
      var referencingTable = GetTable(auxiliaryType);
      if (referencingTable==null)
        return;
      foreach (var field in auxiliaryType.Fields.Where(fieldInfo => fieldInfo.IsEntity)) {
        var referencedType = sourceModel.Types[field.ValueType];
        if (!IsValidForeignKeyTarget(referencedType) || !AreMappedToSameDatabase(auxiliaryType, referencedType))
          continue;
        var referencedTable = GetTable(referencedType);
        if (referencedTable==null)
          continue;
        var foreignKeyName = nameBuilder.BuildReferenceForeignKeyName(auxiliaryType, field, referencedType);
        CreateReferenceForeignKey(referencingTable, referencedTable, field, foreignKeyName);
      }
    }

    private bool AreMappedToSameDatabase(TypeInfo type1, TypeInfo type2)
    {
      return type1.MappingDatabase==type2.MappingDatabase;
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

    private StorageTypeInfo CreateType(Type type, int? length, int? precision, int? scale)
    {
      var sqlValueType = driver.MapValueType(type, length, precision, scale);

      return new StorageTypeInfo(
        sqlValueType.Type.ToClrType(), sqlValueType, sqlValueType.Length, sqlValueType.Precision, sqlValueType.Scale);
    }

    private SecondaryIndexInfo CreateSecondaryIndex(TableInfo owningTable, string indexName, IndexInfo originalModelIndex)
    {
      var index = new SecondaryIndexInfo(owningTable, indexName);

      if (originalModelIndex.Filter!=null) {
        if (providerInfo.Supports(ProviderFeatures.PartialIndexes))
          index.Filter = new PartialIndexFilterInfo(compiler.Compile(handlers, originalModelIndex));
        else
          UpgradeLog.Warning(
            Strings.LogStorageXDoesNotSupportPartialIndexesIgnoringFilterForPartialIndexY,
            providerInfo.ProviderName, originalModelIndex);
      }

      return index;
    }

    // Constructors

    public DomainModelConverter(
      HandlerAccessor handlers, ITypeIdProvider typeIdProvider, PartialIndexFilterCompiler compiler, MappingResolver resolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(typeIdProvider, "typeIdProvider");
      ArgumentValidator.EnsureArgumentNotNull(compiler, "compiler");
      ArgumentValidator.EnsureArgumentNotNull(resolver, "resolver");

      this.handlers = handlers;
      this.compiler = compiler;
      this.typeIdProvider = typeIdProvider;
      this.resolver = resolver;

      sourceModel = handlers.Domain.Model;
      configuration = handlers.Domain.Configuration;
      providerInfo = handlers.ProviderInfo;
      driver = handlers.StorageDriver;
      nameBuilder = handlers.NameBuilder;
    }
  }
}