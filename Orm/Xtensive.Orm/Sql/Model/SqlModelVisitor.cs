// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.03.31

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// An abstract base class for all database model visitors. 
  /// </summary>
  public abstract class SqlModelVisitor<TResult> 
    where TResult : class
  {

    /// <summary>
    /// Visits a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>Visit result.</returns>
    /// <exception cref="ArgumentException">Node type is unknown.</exception>
    protected virtual TResult Visit(Node node) =>
      node switch {
        CharacterSet characterSet => VisitCharacterSet(characterSet),
        Collation collation => VisitCollation(collation),
        TemporaryTable temporaryTable => VisitTemporaryTable(temporaryTable),
        Table table => VisitTable(table),
        View view => VisitView(view),
        DataTable dataTable => VisitDataTable(dataTable),
        TableColumn tableColumn => VisitTableColumn(tableColumn),
        ViewColumn viewColumn => VisitViewColumn(viewColumn),
        DataTableColumn dataTableColumn => VisitDataTableColumn(dataTableColumn),
        Domain domain => VisitDomain(domain),
        FullTextIndex ftIndex => VisitFullTextIndex(ftIndex),
        Index index => VisitIndex(index),
        IndexColumn indexColumn => VisitIndexColumn(indexColumn),
        ForeignKey foreignKey => VisitForeignKey(foreignKey),
        PrimaryKey primaryKey => VisitPrimaryKey(primaryKey),
        UniqueConstraint uniqueConstraint => VisitUniqueConstraint(uniqueConstraint),
        CheckConstraint checkConstraint => VisitCheckConstraint(checkConstraint),
        DomainConstraint domainConstraint => VisitDomainConstraint(domainConstraint),
        Constraint constraint => VisitConstraint(constraint),
        Schema schema => VisitSchema(schema),
        Sequence sequence => VisitSequence(sequence),
        SequenceDescriptor sequenceDescriptor => VisitSequenceDescriptor(sequenceDescriptor),
        Catalog catalog => VisitCatalog(catalog),
        Translation translation => VisitTranslation(translation),
        HashPartition hashPartition => VisitHashPartition(hashPartition),
        ListPartition listPartition => VisitListPartition(listPartition),
        RangePartition rangePartition => VisitRangePartition(rangePartition),
        Partition partition => VisitPartition(partition),
        PartitionDescriptor partitionDescriptor => VisitPartitionDescriptor(partitionDescriptor),
        PartitionFunction partitionFunction => VisitPartitionFunction(partitionFunction),
        PartitionSchema partitionSchema => VisitPartitionSchema(partitionSchema),
        _ => throw new ArgumentException(Strings.ExNodeTypeIsUnknown, "node")
      };

    /// <summary>
    /// Visits unique constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitUniqueConstraint(UniqueConstraint constraint);

    /// <summary>
    /// Visits table constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTableConstraint(TableConstraint constraint);

    /// <summary>
    /// Visits primary key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitPrimaryKey(PrimaryKey key);

    /// <summary>
    /// Visits foreign key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitForeignKey(ForeignKey key);

    /// <summary>
    /// Visits domain constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDomainConstraint(DomainConstraint constraint);

    /// <summary>
    /// Visits a constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitConstraint(Constraint constraint);

    /// <summary>
    /// Visits check constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitCheckConstraint(CheckConstraint constraint);

    /// <summary>
    /// Visits range partition.
    /// </summary>
    /// <param name="rangePartition">The range partition.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitRangePartition(RangePartition rangePartition);

    /// <summary>
    /// Visits partition schema.
    /// </summary>
    /// <param name="partitionSchema">The partition schema.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitPartitionSchema(PartitionSchema partitionSchema);

    /// <summary>
    /// Visits partition function.
    /// </summary>
    /// <param name="partitionFunction">The partition function.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitPartitionFunction(PartitionFunction partitionFunction);

    /// <summary>
    /// Visits partition descriptor.
    /// </summary>
    /// <param name="partitionDescriptor">The partition descriptor.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitPartitionDescriptor(PartitionDescriptor partitionDescriptor);

    /// <summary>
    /// Visits a partition.
    /// </summary>
    /// <param name="partition">The partition.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitPartition(Partition partition);

    /// <summary>
    /// Visits list partition.
    /// </summary>
    /// <param name="listPartition">The list partition.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitListPartition(ListPartition listPartition);

    /// <summary>
    /// Visits hash partition.
    /// </summary>
    /// <param name="hashPartition">The hash partition.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitHashPartition(HashPartition hashPartition);

    /// <summary>
    /// Visits a catalog.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitCatalog(Catalog catalog);

    /// <summary>
    /// Visits character set.
    /// </summary>
    /// <param name="characterSet">The character set.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitCharacterSet(CharacterSet characterSet);

    /// <summary>
    /// Visits a collation.
    /// </summary>
    /// <param name="collation">The collation.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitCollation(Collation collation);

    /// <summary>
    /// Visits data table.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDataTable(DataTable dataTable);

    /// <summary>
    /// Visits data table column.
    /// </summary>
    /// <param name="dataTableColumn">The data table column.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDataTableColumn(DataTableColumn dataTableColumn);

    /// <summary>
    /// Visits a domain.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDomain(Domain domain);

    /// <summary>
    /// Visits the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitFullTextIndex(FullTextIndex index);

    /// <summary>
    /// Visits an index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitIndex(Index index);

    /// <summary>
    /// Visits index column.
    /// </summary>
    /// <param name="indexColumn">The index column.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitIndexColumn(IndexColumn indexColumn);

    /// <summary>
    /// Visits a schema.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitSchema(Schema schema);

    /// <summary>
    /// Visits a sequence.
    /// </summary>
    /// <param name="sequence">The sequence.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitSequence(Sequence sequence);

    /// <summary>
    /// Visits sequence descriptor.
    /// </summary>
    /// <param name="sequenceDescriptor">The sequence descriptor.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitSequenceDescriptor(SequenceDescriptor sequenceDescriptor);

    /// <summary>
    /// Visits a table.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTable(Table table);

    /// <summary>
    /// Visits table column.
    /// </summary>
    /// <param name="tableColumn">The table column.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTableColumn(TableColumn tableColumn);

    /// <summary>
    /// Visits temporary table.
    /// </summary>
    /// <param name="temporaryTable">The temporary table.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTemporaryTable(TemporaryTable temporaryTable);

    /// <summary>
    /// Visits a translation.
    /// </summary>
    /// <param name="translation">The translation.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTranslation(Translation translation);

    /// <summary>
    /// Visits a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitView(View view);

    /// <summary>
    /// Visits view column.
    /// </summary>
    /// <param name="viewColumn">The view column.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitViewColumn(ViewColumn viewColumn);
  }
}