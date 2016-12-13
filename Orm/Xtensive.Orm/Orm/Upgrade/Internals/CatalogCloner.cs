// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

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

      foreach (var partitionSchema in source.PartitionSchemas) {
        newCatalog.CreatePartitionSchema(partitionSchema.Name, pfMap[partitionSchema.PartitionFunction], partitionSchema.Filegroups.ToArray());
      }
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

    private void CloneAssertions(Schema newSchema, Schema oldSchema)
    {
      foreach (var assertion in oldSchema.Assertions) {
        var newAssertion = newSchema.CreateAssertion(assertion.Name, (SqlExpression)assertion.Condition.Clone(), assertion.IsDeferrable, assertion.IsInitiallyDeferred);
        CopyDbName(newAssertion, assertion);
      }
    }

    private void CloneCharacterSets(Schema newSchema, Schema oldSchema)
    {
      foreach (var characterSet in oldSchema.CharacterSets) {
        var newCharacterSet = newSchema.CreateCharacterSet(characterSet.Name);
        CopyDbName(newCharacterSet, characterSet);
      }
    }

    private void CloneCollations(Schema newSchema, Schema oldSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var collation in oldSchema.Collations)
      {
        var newCollation = newSchema.CreateCollation(collation.Name);
        collationsMap.Add(collation, newCollation);
      }
    }

    private void CloneDomains(Schema newSchema, Schema oldSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var domain in oldSchema.Domains) {
        var newDomain = newSchema.CreateDomain(domain.Name, domain.DataType);
        CopyDbName(newDomain, domain);
        newDomain.Collation = collationsMap[domain.Collation];
        newDomain.DefaultValue = (SqlExpression) domain.DefaultValue.Clone();
        foreach (var domainConstraint in domain.DomainConstraints) {
          var newConstraint = newDomain.CreateConstraint(domainConstraint.Name, (SqlExpression) domainConstraint.Condition.Clone());
          CopyDbName(newConstraint, domainConstraint);
        }
      }
    }

    private void CloneSequences(Schema newSchema, Schema oldSchema)
    {
      foreach (var sequence in oldSchema.Sequences) {
        var newSequence = newSchema.CreateSequence(sequence.Name);
        CopyDbName(newSequence, sequence);
        newSequence.SequenceDescriptor = (SequenceDescriptor) sequence.SequenceDescriptor.Clone();
      }
    }

    private void CloneTables(Schema newSchema, Schema oldSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var table in oldSchema.Tables) {
        var newTable = newSchema.CreateTable(table.Name);
        CopyDbName(newTable, table);
        newTable.Filegroup = table.Filegroup;

        CloneTableColumns(newTable, table, collationsMap);
        ClonePartitionDescriptor(newTable, table);
        CloneTableConstraints(newTable, table);
        CloneIndexes(newTable, table);
      }
    }

    private void CloneTranslations(Schema newSchema, Schema oldSchema)
    {
      foreach (var translation in oldSchema.Translations) {
        var newTranslation = newSchema.CreateTranslation(translation.Name);
        CopyDbName(newTranslation, translation);
      }
    }

    private void CloneViews(Schema newSchema, Schema oldSchema)
    {
      foreach (var view in oldSchema.Views) {
        var newView = newSchema.CreateView(view.Name);
        CopyDbName(newView, view);
        newView.CheckOptions = view.CheckOptions;
        newView.Definition = (SqlNative) view.Definition.Clone();
        CloneViewColumns(newView, view);
        CloneIndexes(newView, view);
      }
    }

    private void CloneViewColumns(View newView, View oldView)
    {
      foreach (var viewColumn in oldView.ViewColumns) {
        var newColumn = newView.CreateColumn(viewColumn.Name);
        CopyDbName(newColumn, viewColumn);
      }
    }

    private void CloneTableColumns(Table newTable, Table oldTable, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var tableColumn in oldTable.TableColumns) {
        var newColumn = newTable.CreateColumn(tableColumn.Name, tableColumn.DataType);
        CopyDbName(newColumn, tableColumn);

        if (tableColumn.DefaultValue!=null)
          newColumn.DefaultValue = (SqlExpression) tableColumn.DefaultValue.Clone();

        var schema = newTable.Schema;
        if (tableColumn.Collation!=null) {
          Collation collation;
          if (collationsMap.TryGetValue(tableColumn.Collation, out collation))
            newColumn.Collation = collation;
          else {
            newColumn.Collation = schema.CreateCollation(tableColumn.Collation.Name);
            collationsMap.Add(tableColumn.Collation, newColumn.Collation);
          }
        }
        if (tableColumn.Domain!=null)
          newColumn.Domain = schema.Domains[tableColumn.Domain.Name];
        if (tableColumn.Expression!=null)
          newColumn.Expression = (SqlExpression) tableColumn.Expression.Clone();
        newColumn.IsNullable = tableColumn.IsNullable;
        newColumn.IsPersisted = tableColumn.IsPersisted;
        if (tableColumn.SequenceDescriptor!=null)
          newColumn.SequenceDescriptor = (SequenceDescriptor) tableColumn.SequenceDescriptor.Clone();
      }
    }

    private void CloneTableConstraints(Table newTable, Table oldTable)
    {
      foreach (var tableConstraint in oldTable.TableConstraints)
        CloneTableConstraint(newTable, tableConstraint);
    }

    private void CloneForeignKeys(Schema newSchema, Schema oldSchema)
    {
      foreach (var table in oldSchema.Tables) {
        var newTable = newSchema.Tables[table.Name];
        foreach (var foreignKey in table.TableConstraints.OfType<ForeignKey>()) {
          var fk = newTable.CreateForeignKey(foreignKey.Name);
          CopyDbName(fk, foreignKey);
          fk.Columns.AddRange(foreignKey.Columns.Select(el => newTable.TableColumns[el.Name]));
          fk.MatchType = foreignKey.MatchType;
          fk.OnDelete = foreignKey.OnDelete;
          fk.OnUpdate = foreignKey.OnUpdate;
          var referencedTable = fk.ReferencedTable = newSchema.Tables[foreignKey.ReferencedTable.Name];
          fk.ReferencedColumns.AddRange(foreignKey.ReferencedColumns.Select(el => referencedTable.TableColumns[el.Name]));
        }
      }
    }

    private void CloneIndexes(DataTable newTable, DataTable oldTable)
    {
      foreach (var index in oldTable.Indexes)
        CloneIndex(newTable, index);
    }

    private void CloneIndex(DataTable newTable, Index oldIndex)
    {
      var ftIndex = oldIndex as FullTextIndex;
      if (ftIndex!=null) {
        var ft = newTable.CreateFullTextIndex(ftIndex.Name);
        CopyDbName(ft, ftIndex);
        foreach (var tableColumn in GetKeyColumns(newTable, oldIndex))
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
        ClonePartitionDescriptor(ft, oldIndex);
        return;
      }
      var spatialIndex = oldIndex as SpatialIndex;
      if (spatialIndex!=null) {
        var spatial = newTable.CreateSpatialIndex(spatialIndex.Name);
        CopyDbName(spatial, spatialIndex);
        foreach (var tableColumn in GetKeyColumns(newTable, oldIndex))
          spatial.CreateIndexColumn(tableColumn);

        spatial.NonkeyColumns.AddRange(GetNonKeyColumns(newTable, spatial));
        spatial.Filegroup = spatialIndex.Filegroup;
        spatial.FillFactor = spatialIndex.FillFactor;
        spatial.IsBitmap = spatialIndex.IsBitmap;
        spatial.IsClustered = spatialIndex.IsClustered;
        spatial.IsUnique = spatialIndex.IsUnique;
        if (spatialIndex.Where!=null)
          spatial.Where = (SqlExpression) ftIndex.Where.Clone();
        ClonePartitionDescriptor(spatialIndex, oldIndex);
        return;
      }
      var index = newTable.CreateIndex(oldIndex.Name);
      CopyDbName(index, oldIndex);
      foreach (var tableColumn in GetKeyColumns(newTable, index))
        index.CreateIndexColumn(tableColumn);

      index.Filegroup = oldIndex.Filegroup;
      index.FillFactor = oldIndex.FillFactor;
      index.IsUnique = oldIndex.IsUnique;
      index.IsClustered = oldIndex.IsClustered;
      if (oldIndex.Where!=null)
        index.Where = (SqlExpression) oldIndex.Where.Clone();
      index.NonkeyColumns.AddRange(GetNonKeyColumns(newTable, index));
      index.IsBitmap = oldIndex.IsBitmap;
      ClonePartitionDescriptor(index, oldIndex);
    }

    private DataTableColumn[] GetKeyColumns(DataTable newTable, Index index)
    {
      var table = newTable as Table;
      if (table!=null)
        return index.Columns.Select(el => table.TableColumns[el.Column.Name]).Cast<DataTableColumn>().ToArray();

      var view = newTable as View;
      if (view!=null)
        return index.Columns.Select(el => view.ViewColumns[el.Column.Name]).Cast<DataTableColumn>().ToArray();

      throw new ArgumentOutOfRangeException("newTable", "Unexpected type of parameter.");
    }

    private DataTableColumn[] GetNonKeyColumns(DataTable newTable, Index index)
    {
      var table = newTable as Table;
      if (table!=null) {
        return index.NonkeyColumns.Select(el => table.TableColumns[el.Name]).Cast<DataTableColumn>().ToArray();
      }
      var view = newTable as View;
      if (view!=null) {
        return index.NonkeyColumns.Select(el => view.ViewColumns[el.Name]).Cast<DataTableColumn>().ToArray();
      }
      throw new ArgumentOutOfRangeException("newTable", "Unexpected type of parameter.");
    }

    private void CloneTableConstraint(Table newTable, TableConstraint constraint)
    {
      var checkConstraint = constraint as CheckConstraint;
      if (checkConstraint!=null) {
        var c = newTable.CreateCheckConstraint(checkConstraint.Name, (SqlExpression) checkConstraint.Condition.Clone());
        CopyDbName(c, checkConstraint);
        return;
      }
      var defaultConstraint = constraint as DefaultConstraint;
      if (defaultConstraint!=null) {
        var c = newTable.CreateDefaultConstraint(defaultConstraint.Name, newTable.TableColumns[defaultConstraint.Column.Name]);
        c.NameIsStale = defaultConstraint.NameIsStale;
        CopyDbName(c, defaultConstraint);
      }

      var foreignKey = constraint as ForeignKey;
      if (foreignKey!=null)
        return;

      var uniqueConstraint = constraint as UniqueConstraint;
      if (uniqueConstraint!=null) {
        var primaryKey = constraint as PrimaryKey;
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
      throw new ArgumentOutOfRangeException("constraint", "Unextected type of constraint.");
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
      throw new ArgumentOutOfRangeException("oldPartition", "Unextected type of partition.");
    }

    private TableColumn GetPartitionColumn(IPartitionable newObject, PartitionDescriptor oldPartitionDescriptor)
    {
      var table = newObject as Table;
      if (table!=null) {
        return table.TableColumns[oldPartitionDescriptor.Column.Name];
      }
      var index = newObject as Index;
      if (index!=null) {
        var tableColumn = index.Columns[oldPartitionDescriptor.Column.Name].Column as TableColumn;
        if (tableColumn!=null)
          return tableColumn;
        throw new InvalidOperationException("Unable to get TableColumn instance from index.");
      }
      throw new ArgumentOutOfRangeException("newObject", "Unexpected type of argument.");
    }

    private void CopyDbName(Node newNode, Node sourceNode)
    {
      if (sourceNode.DbName!=sourceNode.Name)
        newNode.DbName = sourceNode.DbName;
    }
  }
}