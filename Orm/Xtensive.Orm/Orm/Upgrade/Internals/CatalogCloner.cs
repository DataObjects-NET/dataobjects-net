// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class CatalogCloner
  {
    private const char NameElementSeparator = ':';

    public Catalog Clone(Catalog source, MappingResolver mappingResolver, string newCatalogName)
    {
      var newCatalog = new Catalog(newCatalogName);

      var schemaMap = CloneSchemas(newCatalog, source, mappingResolver);
      ClonePartitionFuctionsAndSchemas(newCatalog, source);
      CloneSchemas(newCatalog, source, schemaMap);
      return newCatalog;
    }

    private Dictionary<string, string> CloneSchemas(Catalog newCatalog, Catalog sourceCatalog, MappingResolver mappingResolver)
    {
      var schemaMap = new Dictionary<string, string>();
      foreach (var schema in sourceCatalog.Schemas) {
        var complexName = mappingResolver.GetNodeName(newCatalog.Name, schema.Name, "Dummy");
        var names = complexName.Split(NameElementSeparator);
        var schemaName = schema.Name;
        switch (names.Length) {
          case 3:
            schemaName = names[1];
            break;
          case 2:
            schemaName = names[0];
            break;
        }
        schemaMap.Add(schema.Name, schemaName);
      }
      return schemaMap;
    }

    private void ClonePartitionFuctionsAndSchemas(Catalog newCatalog, Catalog source)
    {
      var pfMap = new Dictionary<PartitionFunction, PartitionFunction>();
      foreach (var partitionFunction in source.PartitionFunctions) {
        var newFunction = newCatalog.CreatePartitionFunction(partitionFunction.Name, partitionFunction.DataType, partitionFunction.BoundaryValues);
        CopyDbName(newFunction, partitionFunction);
        pfMap.Add(partitionFunction, newFunction);
      }

      foreach (var partitionSchema in source.PartitionSchemas)
        newCatalog.CreatePartitionSchema(partitionSchema.Name, pfMap[partitionSchema.PartitionFunction], partitionSchema.Filegroups.ToArray());
    }

    private void CloneSchemas(Catalog newCatalog, Catalog source, Dictionary<string, string> schemaMap)
    {
      foreach (var schema in source.Schemas) {
        var newSchema = newCatalog.CreateSchema(schemaMap[schema.Name]);
        CopyDbName(newSchema, schema);
        CloneAssertions(newSchema, schema);
        CloneCharacterSets(newSchema, schema);

        var collationsMap = new Dictionary<Collation, Collation>();
        CloneCollations(newSchema, schema, collationsMap);
        CloneDomains(newSchema, schema, collationsMap);
        CloneSequences(newSchema, schema);
        CloneTables(newSchema, schema, collationsMap);
        CloneForeignKeys(newSchema, schema);
        CloneTranslations(newSchema, schema);
        CloneViews(newSchema, schema);
      }
    }

    private void CloneAssertions(Schema newSchema, Schema sourceSchema)
    {
      foreach (var assertion in sourceSchema.Assertions) {
        var newAssertion = newSchema.CreateAssertion(assertion.Name, (SqlExpression)assertion.Condition.Clone(), assertion.IsDeferrable, assertion.IsInitiallyDeferred);
        CopyDbName(newAssertion, assertion);
      }
    }

    private void CloneCharacterSets(Schema newSchema, Schema sourceSchema)
    {
      foreach (var characterSet in sourceSchema.CharacterSets) {
        var newCharacterSet = newSchema.CreateCharacterSet(characterSet.Name);
        CopyDbName(newCharacterSet, characterSet);
      }
    }

    private void CloneCollations(Schema newSchema, Schema sourceSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var collation in sourceSchema.Collations) {
        var newCollation = newSchema.CreateCollation(collation.Name);
        collationsMap.Add(collation, newCollation);
      }
    }

    private void CloneDomains(Schema newSchema, Schema sourceSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var sourceDomain in sourceSchema.Domains) {
        var newDomain = newSchema.CreateDomain(sourceDomain.Name, sourceDomain.DataType);
        CopyDbName(newDomain, sourceDomain);
        if (sourceDomain.Collation!=null)
          newDomain.Collation = collationsMap[sourceDomain.Collation];
        if (sourceDomain.DefaultValue!=null)
          newDomain.DefaultValue = (SqlExpression)sourceDomain.DefaultValue.Clone();
        foreach (var domainConstraint in sourceDomain.DomainConstraints) {
          var newConstraint = newDomain.CreateConstraint(domainConstraint.Name, (SqlExpression) domainConstraint.Condition.Clone());
          CopyDbName(newConstraint, domainConstraint);
        }
      }
    }

    private void CloneSequences(Schema newSchema, Schema sourceSchema)
    {
      foreach (var sourceSequence in sourceSchema.Sequences) {
        var newSequence = newSchema.CreateSequence(sourceSequence.Name);
        CopyDbName(newSequence, sourceSequence);
        newSequence.SequenceDescriptor = (SequenceDescriptor) sourceSequence.SequenceDescriptor.Clone();
      }
    }

    private void CloneTables(Schema newSchema, Schema sourceSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var sourceTable in sourceSchema.Tables) {
        var newTable = newSchema.CreateTable(sourceTable.Name);
        CopyDbName(newTable, sourceTable);
        newTable.Filegroup = sourceTable.Filegroup;

        CloneTableColumns(newTable, sourceTable, collationsMap);
        ClonePartitionDescriptor(newTable, sourceTable);
        CloneTableConstraints(newTable, sourceTable);
        CloneIndexes(newTable, sourceTable);
      }
    }

    private void CloneTranslations(Schema newSchema, Schema sourceSchema)
    {
      foreach (var translation in sourceSchema.Translations) {
        var newTranslation = newSchema.CreateTranslation(translation.Name);
        CopyDbName(newTranslation, translation);
      }
    }

    private void CloneViews(Schema newSchema, Schema sourceSchema)
    {
      foreach (var sourceView in sourceSchema.Views) {
        var newView = newSchema.CreateView(sourceView.Name);
        CopyDbName(newView, sourceView);
        newView.CheckOptions = sourceView.CheckOptions;
        newView.Definition = (SqlNative) sourceView.Definition.Clone();
        CloneViewColumns(newView, sourceView);
        CloneIndexes(newView, sourceView);
      }
    }

    private void CloneViewColumns(View newView, View sourceView)
    {
      foreach (var sourceViewColumn in sourceView.ViewColumns) {
        var newColumn = newView.CreateColumn(sourceViewColumn.Name);
        CopyDbName(newColumn, sourceViewColumn);
      }
    }

    private void CloneTableColumns(Table newTable, Table sourceTable, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var sourceTableColumn in sourceTable.TableColumns) {
        var newColumn = newTable.CreateColumn(sourceTableColumn.Name, sourceTableColumn.DataType);
        CopyDbName(newColumn, sourceTableColumn);

        if (sourceTableColumn.DefaultValue!=null)
          newColumn.DefaultValue = (SqlExpression) sourceTableColumn.DefaultValue.Clone();

        var schema = newTable.Schema;
        if (sourceTableColumn.Collation!=null) {
          Collation collation;
          if (collationsMap.TryGetValue(sourceTableColumn.Collation, out collation))
            newColumn.Collation = collation;
          else {
            newColumn.Collation = schema.CreateCollation(sourceTableColumn.Collation.Name);
            collationsMap.Add(sourceTableColumn.Collation, newColumn.Collation);
          }
        }
        if (sourceTableColumn.Domain!=null)
          newColumn.Domain = schema.Domains[sourceTableColumn.Domain.Name];
        if (sourceTableColumn.Expression!=null)
          newColumn.Expression = (SqlExpression) sourceTableColumn.Expression.Clone();
        newColumn.IsNullable = sourceTableColumn.IsNullable;
        newColumn.IsPersisted = sourceTableColumn.IsPersisted;
        if (sourceTableColumn.SequenceDescriptor!=null)
          newColumn.SequenceDescriptor = (SequenceDescriptor) sourceTableColumn.SequenceDescriptor.Clone();
      }
    }

    private void CloneTableConstraints(Table newTable, Table sourceTable)
    {
      foreach (var tableConstraint in sourceTable.TableConstraints)
        CloneTableConstraint(newTable, tableConstraint);
    }

    private void CloneForeignKeys(Schema newSchema, Schema sourceSchema)
    {
      foreach (var table in sourceSchema.Tables) {
        var newTable = newSchema.Tables[table.Name];
        foreach (var sourceForeignKey in table.TableConstraints.OfType<ForeignKey>()) {
          var newForeignKey = newTable.CreateForeignKey(sourceForeignKey.Name);
          CopyDbName(newForeignKey, sourceForeignKey);
          newForeignKey.Columns.AddRange(sourceForeignKey.Columns.Select(el => newTable.TableColumns[el.Name]));
          newForeignKey.MatchType = sourceForeignKey.MatchType;
          newForeignKey.OnDelete = sourceForeignKey.OnDelete;
          newForeignKey.OnUpdate = sourceForeignKey.OnUpdate;
          var referencedTable = newForeignKey.ReferencedTable = newSchema.Tables[sourceForeignKey.ReferencedTable.Name];
          newForeignKey.ReferencedColumns.AddRange(sourceForeignKey.ReferencedColumns.Select(el => referencedTable.TableColumns[el.Name]));
        }
      }
    }

    private void CloneIndexes(DataTable newTable, DataTable sourceTable)
    {
      foreach (var index in sourceTable.Indexes)
        CloneIndex(newTable, index);
    }

    private void CloneIndex(DataTable newTable, Index sourceIndex)
    {
      var ftIndex = sourceIndex as FullTextIndex;
      if (ftIndex!=null) {
        var ft = newTable.CreateFullTextIndex(ftIndex.Name);
        CopyDbName(ft, ftIndex);
        foreach (var tableColumn in GetKeyColumns(newTable, sourceIndex))
          ft.CreateIndexColumn(tableColumn);

        ft.NonkeyColumns.AddRange(GetNonKeyColumns(newTable, ft));
        ft.Filegroup = ftIndex.Filegroup;
        ft.FillFactor = ftIndex.FillFactor;
        ft.FullTextCatalog = ftIndex.FullTextCatalog;
        ft.IsBitmap = ftIndex.IsBitmap;
        ft.IsClustered = ftIndex.IsClustered;
        ft.IsUnique = ftIndex.IsUnique;
        ft.UnderlyingUniqueIndex = ftIndex.UnderlyingUniqueIndex;
        if (ftIndex.Where!=null)
          ft.Where = (SqlExpression) ftIndex.Where.Clone();
        ClonePartitionDescriptor(ft, sourceIndex);
        return;
      }
      var spatialIndex = sourceIndex as SpatialIndex;
      if (spatialIndex!=null) {
        var spatial = newTable.CreateSpatialIndex(spatialIndex.Name);
        CopyDbName(spatial, spatialIndex);
        foreach (var tableColumn in GetKeyColumns(newTable, sourceIndex))
          spatial.CreateIndexColumn(tableColumn);

        spatial.NonkeyColumns.AddRange(GetNonKeyColumns(newTable, spatial));
        spatial.Filegroup = spatialIndex.Filegroup;
        spatial.FillFactor = spatialIndex.FillFactor;
        spatial.IsBitmap = spatialIndex.IsBitmap;
        spatial.IsClustered = spatialIndex.IsClustered;
        spatial.IsUnique = spatialIndex.IsUnique;
        if (spatialIndex.Where!=null)
          spatial.Where = (SqlExpression) spatialIndex.Where.Clone();
        ClonePartitionDescriptor(spatialIndex, sourceIndex);
        return;
      }
      var index = newTable.CreateIndex(sourceIndex.Name);
      CopyDbName(index, sourceIndex);
      foreach (var tableColumn in GetKeyColumns(newTable, sourceIndex))
        index.CreateIndexColumn(tableColumn);

      index.Filegroup = sourceIndex.Filegroup;
      index.FillFactor = sourceIndex.FillFactor;
      index.IsUnique = sourceIndex.IsUnique;
      index.IsClustered = sourceIndex.IsClustered;
      if (sourceIndex.Where!=null)
        index.Where = (SqlExpression) sourceIndex.Where.Clone();
      index.NonkeyColumns.AddRange(GetNonKeyColumns(newTable, sourceIndex));
      index.IsBitmap = sourceIndex.IsBitmap;
      ClonePartitionDescriptor(index, sourceIndex);
    }

    private DataTableColumn[] GetKeyColumns(DataTable newTable, Index sourceIndex)
    {
      var table = newTable as Table;
      if (table!=null)
        return sourceIndex.Columns.Select(el => table.TableColumns[el.Column.Name]).Cast<DataTableColumn>().ToArray();

      var view = newTable as View;
      if (view!=null)
        return sourceIndex.Columns.Select(el => view.ViewColumns[el.Column.Name]).Cast<DataTableColumn>().ToArray();

      throw new ArgumentOutOfRangeException("newTable", Strings.ExUnexpectedTypeOfParameter);
    }

    private DataTableColumn[] GetNonKeyColumns(DataTable newTable, Index sourceIndex)
    {
      var table = newTable as Table;
      if (table!=null) 
        return sourceIndex.NonkeyColumns.Select(el => table.TableColumns[el.Name]).Cast<DataTableColumn>().ToArray();

      var view = newTable as View;
      if (view!=null) 
        return sourceIndex.NonkeyColumns.Select(el => view.ViewColumns[el.Name]).Cast<DataTableColumn>().ToArray();

      throw new ArgumentOutOfRangeException("newTable", Strings.ExUnexpectedTypeOfParameter);
    }

    private void CloneTableConstraint(Table newTable, TableConstraint sourceConstraint)
    {
      var checkConstraint = sourceConstraint as CheckConstraint;
      if (checkConstraint!=null) {
        var c = newTable.CreateCheckConstraint(checkConstraint.Name, (SqlExpression) checkConstraint.Condition.Clone());
        CopyDbName(c, checkConstraint);
        return;
      }

      var defaultConstraint = sourceConstraint as DefaultConstraint;
      if (defaultConstraint!=null) {
        var c = newTable.CreateDefaultConstraint(defaultConstraint.Name, newTable.TableColumns[defaultConstraint.Column.Name]);
        c.NameIsStale = defaultConstraint.NameIsStale;
        CopyDbName(c, defaultConstraint);
      }

      //foreign keys are handled by special method
      if (sourceConstraint is ForeignKey) {
        return;
      }

      var uniqueConstraint = sourceConstraint as UniqueConstraint;
      if (uniqueConstraint!=null) {
        var primaryKey = sourceConstraint as PrimaryKey;
        if (primaryKey!=null) {
          var columns = primaryKey.Columns.Select(c => newTable.TableColumns[c.Name]).ToArray();
          var pk =newTable.CreatePrimaryKey(primaryKey.Name, columns);
          CopyDbName(pk, primaryKey);
        }
        else {
          var columns = uniqueConstraint.Columns.Select(c => newTable.TableColumns[c.Name]).ToArray();
          var uc = newTable.CreateUniqueConstraint(uniqueConstraint.Name, columns);
          CopyDbName(uc, uniqueConstraint);
        }
        return;
      }
      throw new ArgumentOutOfRangeException("sourceConstraint", Strings.ExUnexpectedTypeOfParameter);
    }

    private void ClonePartitionDescriptor(IPartitionable newObject, IPartitionable oldObject)
    {
      var oldPartitionDescriptor = oldObject.PartitionDescriptor;
      if (oldPartitionDescriptor==null)
        return;

      var column = GetPartitionColumn(newObject, oldPartitionDescriptor);
      var partitionDescriptor = new PartitionDescriptor(newObject, column, oldPartitionDescriptor.PartitionMethod);
      CopyDbName(partitionDescriptor, oldPartitionDescriptor);
      foreach (var oldPartition in oldPartitionDescriptor.Partitions)
        ClonePartition(partitionDescriptor, oldPartition);

      newObject.PartitionDescriptor = partitionDescriptor;
    }

    private void ClonePartition(PartitionDescriptor newPartitionDescriptor, Partition oldPartition)
    {
      var hashPartition = oldPartition as HashPartition;
      if (hashPartition!=null) {
        var newPartition = newPartitionDescriptor.CreateHashPartition(hashPartition.Filegroup);
        CopyDbName(oldPartition, newPartition);
        return;
      }
      var listPartition = oldPartition as ListPartition;
      if (listPartition!=null) {
        var newPartition = newPartitionDescriptor.CreateListPartition(listPartition.Filegroup, (string[]) listPartition.Values.Clone());
        CopyDbName(oldPartition, newPartition);
        return;
      }
      var rangePartition = oldPartition as RangePartition;
      if (rangePartition!=null) {
        var newPartition = newPartitionDescriptor.CreateRangePartition(rangePartition.Filegroup, rangePartition.Boundary);
        CopyDbName(oldPartition, newPartition);
        return;
      }
      throw new ArgumentOutOfRangeException("oldPartition", Strings.ExUnexpectedTypeOfParameter);
    }

    private TableColumn GetPartitionColumn(IPartitionable newObject, PartitionDescriptor oldPartitionDescriptor)
    {
      var table = newObject as Table;
      if (table!=null)
        return table.TableColumns[oldPartitionDescriptor.Column.Name];

      var index = newObject as Index;
      if (index!=null) {
        var tableColumn = index.Columns[oldPartitionDescriptor.Column.Name].Column as TableColumn;
        if (tableColumn!=null)
          return tableColumn;
        throw new InvalidOperationException(Strings.ExUnableToGetTableColumnInstanceFromIndex);
      }
      throw new ArgumentOutOfRangeException("newObject", Strings.ExUnexpectedTypeOfParameter);
    }

    private void CopyDbName(Node newNode, Node sourceNode)
    {
      if (sourceNode.DbName!=sourceNode.Name)
        newNode.DbName = sourceNode.DbName;
    }
  }
}