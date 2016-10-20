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

    public Catalog Clone(Catalog source, MappingResolver mappingResolver)
    {
      string newCatalogName;
      Dictionary<string, string> schemaMap;
      CreateSchemaMappings(source, mappingResolver, out schemaMap, out newCatalogName);

      var newCatalog = new Catalog(newCatalogName);
      if (source.DbName!=source.Name)
        newCatalog.DbName = source.DbName;

      ClonePartitionFuctionsAndSchemas(newCatalog, source);
      CloneSchemas(newCatalog, source, schemaMap);
      return newCatalog;
    }

    private void CreateSchemaMappings(Catalog catalog, MappingResolver mappingResolver, out Dictionary<string, string> schemaMap, out string catalogName)
    {
      schemaMap = new Dictionary<string, string>();
      catalogName = string.Empty;
      foreach (var schema in catalog.Schemas) {
        var name = mappingResolver.GetNodeName(catalog.Name, schema.Name, "Dummy");
        var names = name.Split(NameElementSeparator);
        var schemaName = schema.Name;
        if (names.Length==3) {
          catalogName = names[0];
          schemaName = names[1];
        }
        else if (names.Length==2)
        {
          schemaName = names[0];
        }
        schemaMap.Add(schema.Name, schemaName);
      }
      if (catalogName.IsNullOrEmpty())
        catalogName = catalog.Name;
    }

    private void ClonePartitionFuctionsAndSchemas(Catalog newCatalog, Catalog source)
    {
      var pfMap = new Dictionary<PartitionFunction, PartitionFunction>();
      foreach (var partitionFunction in source.PartitionFunctions) {
        var newFunction = newCatalog.CreatePartitionFunction(partitionFunction.Name, partitionFunction.DataType, partitionFunction.BoundaryValues);
        if (partitionFunction.DbName!=partitionFunction.Name)
          newFunction.DbName = partitionFunction.DbName;
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
      foreach (var assertion in oldSchema.Assertions)
        newSchema.CreateAssertion(assertion.Name, (SqlExpression)assertion.Condition.Clone(), assertion.IsDeferrable, assertion.IsInitiallyDeferred);
    }

    private void CloneCharacterSets(Schema newSchema, Schema oldSchema)
    {
      foreach (var characterSet in oldSchema.CharacterSets)
        newSchema.CreateCharacterSet(characterSet.Name);
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
        newDomain.Collation = collationsMap[domain.Collation];
        newDomain.DefaultValue = (SqlExpression) domain.DefaultValue.Clone();
        foreach (var domainConstraint in domain.DomainConstraints)
          newDomain.CreateConstraint(domainConstraint.Name, (SqlExpression) domainConstraint.Condition.Clone());
      }
    }

    private void CloneSequences(Schema newSchema, Schema oldSchema)
    {
      foreach (var sequence in oldSchema.Sequences) {
        var newSequence = newSchema.CreateSequence(sequence.Name);
        newSequence.SequenceDescriptor = (SequenceDescriptor) sequence.SequenceDescriptor.Clone();
      }
    }

    private void CloneTables(Schema newSchema, Schema oldSchema, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var table in oldSchema.Tables) {
        var newTable = newSchema.CreateTable(table.Name);
        if (table.DbName!=table.Name)
          newTable.DbName = table.DbName;
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
      }
    }

    private void CloneViews(Schema newSchema, Schema oldSchema)
    {
      foreach (var view in oldSchema.Views) {
        var newView = newSchema.CreateView(view.Name);
        if (view.DbName!=view.Name)
          newView.DbName = view.DbName;
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
      }
    }

    private void CloneTableColumns(Table newTable, Table oldTable, Dictionary<Collation, Collation> collationsMap)
    {
      foreach (var tableColumn in oldTable.TableColumns) {
        var newColumn = newTable.CreateColumn(tableColumn.Name, tableColumn.DataType);
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
      foreach (var index in oldTable.Indexes) {
        CloneIndex(newTable, index);
      }
    }

    private void CloneIndex(DataTable newTable, Index oldIndex)
    {
      var ftIndex = oldIndex as FullTextIndex;
      if (ftIndex!=null) {
        var ft = newTable.CreateFullTextIndex(ftIndex.Name);
        foreach (var tableColumn in GetKeyColumns(newTable, oldIndex)) {
          ft.CreateIndexColumn(tableColumn);
        }
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
      foreach (var tableColumn in GetKeyColumns(newTable, index)) {
        index.CreateIndexColumn(tableColumn);
      }
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
      if (table!=null) {
        return index.Columns.Select(el => table.TableColumns[el.Column.Name]).Cast<DataTableColumn>().ToArray();
      }
      var view = newTable as View;
      if (view!=null) {
        return index.Columns.Select(el => view.ViewColumns[el.Column.Name]).Cast<DataTableColumn>().ToArray();
      }
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
        return;
      }
      var defaultConstraint = constraint as DefaultConstraint;
      if (defaultConstraint!=null) {
        var c = newTable.CreateDefaultConstraint(defaultConstraint.Name, newTable.TableColumns[defaultConstraint.Column.Name]);
        c.NameIsStale = defaultConstraint.NameIsStale;
      }
      var foreignKey = constraint as ForeignKey;
      if (foreignKey!=null)
        return;

      var uniqueConstraint = constraint as UniqueConstraint;
      if (uniqueConstraint!=null) {
        var primaryKey = constraint as PrimaryKey;
        if (primaryKey!=null) {
          var columns = primaryKey.Columns.Select(c => newTable.TableColumns[c.Name]).ToArray();
          newTable.CreatePrimaryKey(primaryKey.Name, columns);
        }
        else {
          var columns = uniqueConstraint.Columns.Select(c => newTable.TableColumns[c.Name]).ToArray();
          newTable.CreateUniqueConstraint(uniqueConstraint.Name, columns);
        }
        return;
      }
      throw new ArgumentOutOfRangeException("constraint", "Unextected type of constraint.");
    }

    private void ClonePartitionDescriptor(IPartitionable newObject, IPartitionable oldObject)
    {
      var oldPartitionDescriptor = oldObject.PartitionDescriptor;
      if (oldPartitionDescriptor == null)
        return;

      var column = GetPartitionColumn(newObject, oldPartitionDescriptor);
      var partitionDescriptor = new PartitionDescriptor(newObject, column, oldPartitionDescriptor.PartitionMethod);
      foreach (var oldPartition in oldPartitionDescriptor.Partitions)
      {
        ClonePartition(partitionDescriptor, oldPartition);
      }
      newObject.PartitionDescriptor = partitionDescriptor;
    }

    private void ClonePartition(PartitionDescriptor newPartitionDescriptor, Partition oldPartition)
    {
      var hashPartition = oldPartition as HashPartition;
      if (hashPartition!=null) {
        newPartitionDescriptor.CreateHashPartition(hashPartition.Filegroup);
        return;
      }
      var listPartition = oldPartition as ListPartition;
      if (listPartition!=null) {
        newPartitionDescriptor.CreateListPartition(listPartition.Filegroup, (string[]) listPartition.Values.Clone());
        return;
      }
      var rangePartition = oldPartition as RangePartition;
      if (rangePartition!=null) {
        newPartitionDescriptor.CreateRangePartition(rangePartition.Filegroup, rangePartition.Boundary);
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
  }
}