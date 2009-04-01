namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// An abstract database model visitor. 
  /// </summary>
  public abstract class ModelVisitor<TResult> 
    where TResult : class
  {

    /// <summary>
    /// Visits the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns></returns>
    protected virtual TResult Visit(Node node)
    {
      var characterSet = node as CharacterSet;
      if (characterSet!=null)
        return VisitCharacterSet(characterSet);
      var collation = node as Collation;
      if (collation!=null)
        return VisitCollation(collation);
      var temporaryTable = node as TemporaryTable;
      if (temporaryTable!=null)
        return VisitTemporaryTable(temporaryTable);
      var table = node as Table;
      if (table!=null)
        return VisitTable(table);
      var view = node as View;
      if (view!=null)
        return VisitView(view);
      var dataTable = node as DataTable;
      if (dataTable!=null)
        return VisitDataTable(dataTable);
      var tableColumn = node as TableColumn;
      if (tableColumn!=null)
        return VisitTableColumn(tableColumn);
      var viewColumn = node as ViewColumn;
      if (viewColumn!=null)
        return VisitViewColumn(viewColumn);
      var dataTableColumn = node as DataTableColumn;
      if (dataTableColumn!=null)
        return VisitDataTableColumn(dataTableColumn);
      var domain = node as Domain;
      if (domain!=null)
        return VisitDomain(domain);
      var index = node as Index;
      if (index!=null)
        return VisitIndex(index);
      var indexColumn = node as IndexColumn;
      if (indexColumn!=null)
        return VisitIndexColumn(indexColumn);
      var foreignKey = node as ForeignKey;
      if (foreignKey!=null)
        return VisitForeignKey(foreignKey);
      var primaryKey = node as PrimaryKey;
      if (primaryKey!=null)
        return VisitPrimaryKey(primaryKey);
      var uniqueConstraint = node as UniqueConstraint;
      if (uniqueConstraint!=null)
        return VisitUniqueConstraint(uniqueConstraint);
      var checkConstraint = node as CheckConstraint;
      if (checkConstraint!=null)
        return VisitCheckConstraint(checkConstraint);
      var domainConstraint = node as DomainConstraint;
      if (domainConstraint!=null)
        return VisitDomainConstraint(domainConstraint);
      var constraint = node as Constraint;
      if (constraint!=null)
        return VisitConstraint(constraint);
      var model = node as Model;
      if (model!=null)
        return VisitModel(model);
      var schema = node as Schema;
      if (schema!=null)
        return VisitSchema(schema);
      var sequence = node as Sequence;
      if (sequence!=null)
        return VisitSequence(sequence);
      var sequenceDescriptor = node as SequenceDescriptor;
      if (sequenceDescriptor!=null)
        return VisitSequenceDescriptor(sequenceDescriptor);
      var server = node as Server;
      if (server!=null)
        return VisitServer(server);
      var catalog = node as Catalog;
      if (catalog!=null)
        return VisitCatalog(catalog);
      var translation = node as Translation;
      if (translation!=null)
        return VisitTranslation(translation);
      var user = node as User;
      if (user!=null)
        return VisitUser(user);
      var hashPartition = node as HashPartition;
      if (hashPartition!=null)
        return VisitHashPartition(hashPartition);
      var listPartition = node as ListPartition;
      if (listPartition!=null)
        return VisitListPartition(listPartition);
      var rangePartition = node as RangePartition;
      if (rangePartition!=null)
        return VisitRangePartition(rangePartition);
      var partition = node as Partition;
      if (partition!=null)
        return VisitPartition(partition);
      var partitionDescriptor = node as PartitionDescriptor;
      if (partitionDescriptor!=null)
        return VisitPartitionDescriptor(partitionDescriptor);
      var partitionFunction = node as PartitionFunction;
      if (partitionFunction!=null)
        return VisitPartitionFunction(partitionFunction);
      var partitionSchema = node as PartitionSchema;
      if (partitionSchema!=null)
        return VisitPartitionSchema(partitionSchema);


      return null;
    }

    /// <summary>
    /// Visits the unique constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns></returns>
    protected abstract TResult VisitUniqueConstraint(UniqueConstraint constraint);

    /// <summary>
    /// Visits the table constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns></returns>
    protected abstract TResult VisitTableConstraint(TableConstraint constraint);

    /// <summary>
    /// Visits the primary key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected abstract TResult VisitPrimaryKey(PrimaryKey key);

    /// <summary>
    /// Visits the foreign key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected abstract TResult VisitForeignKey(ForeignKey key);

    /// <summary>
    /// Visits the domain constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns></returns>
    protected abstract TResult VisitDomainConstraint(DomainConstraint constraint);

    /// <summary>
    /// Visits the constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns></returns>
    protected abstract TResult VisitConstraint(Constraint constraint);

    /// <summary>
    /// Visits the check constraint.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <returns></returns>
    protected abstract TResult VisitCheckConstraint(CheckConstraint constraint);

    /// <summary>
    /// Visits the range partition.
    /// </summary>
    /// <param name="rangePartition">The range partition.</param>
    /// <returns></returns>
    protected abstract TResult VisitRangePartition(RangePartition rangePartition);

    /// <summary>
    /// Visits the partition schema.
    /// </summary>
    /// <param name="partitionSchema">The partition schema.</param>
    /// <returns></returns>
    protected abstract TResult VisitPartitionSchema(PartitionSchema partitionSchema);

    /// <summary>
    /// Visits the partition function.
    /// </summary>
    /// <param name="partitionFunction">The partition function.</param>
    /// <returns></returns>
    protected abstract TResult VisitPartitionFunction(PartitionFunction partitionFunction);

    /// <summary>
    /// Visits the partition descriptor.
    /// </summary>
    /// <param name="partitionDescriptor">The partition descriptor.</param>
    /// <returns></returns>
    protected abstract TResult VisitPartitionDescriptor(PartitionDescriptor partitionDescriptor);

    /// <summary>
    /// Visits the partition.
    /// </summary>
    /// <param name="partition">The partition.</param>
    /// <returns></returns>
    protected abstract TResult VisitPartition(Partition partition);

    /// <summary>
    /// Visits the list partition.
    /// </summary>
    /// <param name="listPartition">The list partition.</param>
    /// <returns></returns>
    protected abstract TResult VisitListPartition(ListPartition listPartition);

    /// <summary>
    /// Visits the hash partition.
    /// </summary>
    /// <param name="hashPartition">The hash partition.</param>
    /// <returns></returns>
    protected abstract TResult VisitHashPartition(HashPartition hashPartition);

    /// <summary>
    /// Visits the catalog.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <returns></returns>
    protected abstract TResult VisitCatalog(Catalog catalog);

    /// <summary>
    /// Visits the character set.
    /// </summary>
    /// <param name="characterSet">The character set.</param>
    /// <returns></returns>
    protected abstract TResult VisitCharacterSet(CharacterSet characterSet);

    /// <summary>
    /// Visits the collation.
    /// </summary>
    /// <param name="collation">The collation.</param>
    /// <returns></returns>
    protected abstract TResult VisitCollation(Collation collation);

    /// <summary>
    /// Visits the data table.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <returns></returns>
    protected abstract TResult VisitDataTable(DataTable dataTable);

    /// <summary>
    /// Visits the data table column.
    /// </summary>
    /// <param name="dataTableColumn">The data table column.</param>
    /// <returns></returns>
    protected abstract TResult VisitDataTableColumn(DataTableColumn dataTableColumn);

    /// <summary>
    /// Visits the domain.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns></returns>
    protected abstract TResult VisitDomain(Domain domain);

    /// <summary>
    /// Visits the index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns></returns>
    protected abstract TResult VisitIndex(Index index);

    /// <summary>
    /// Visits the index column.
    /// </summary>
    /// <param name="indexColumn">The index column.</param>
    /// <returns></returns>
    protected abstract TResult VisitIndexColumn(IndexColumn indexColumn);

    /// <summary>
    /// Visits the model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns></returns>
    protected abstract TResult VisitModel(Model model);

    /// <summary>
    /// Visits the schema.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <returns></returns>
    protected abstract TResult VisitSchema(Schema schema);

    /// <summary>
    /// Visits the sequence.
    /// </summary>
    /// <param name="sequence">The sequence.</param>
    /// <returns></returns>
    protected abstract TResult VisitSequence(Sequence sequence);

    /// <summary>
    /// Visits the sequence descriptor.
    /// </summary>
    /// <param name="sequenceDescriptor">The sequence descriptor.</param>
    /// <returns></returns>
    protected abstract TResult VisitSequenceDescriptor(SequenceDescriptor sequenceDescriptor);

    /// <summary>
    /// Visits the server.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns></returns>
    protected abstract TResult VisitServer(Server server);

    /// <summary>
    /// Visits the table.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns></returns>
    protected abstract TResult VisitTable(Table table);

    /// <summary>
    /// Visits the table column.
    /// </summary>
    /// <param name="tableColumn">The table column.</param>
    /// <returns></returns>
    protected abstract TResult VisitTableColumn(TableColumn tableColumn);

    /// <summary>
    /// Visits the temporary table.
    /// </summary>
    /// <param name="temporaryTable">The temporary table.</param>
    /// <returns></returns>
    protected abstract TResult VisitTemporaryTable(TemporaryTable temporaryTable);

    /// <summary>
    /// Visits the translation.
    /// </summary>
    /// <param name="translation">The translation.</param>
    /// <returns></returns>
    protected abstract TResult VisitTranslation(Translation translation);

    /// <summary>
    /// Visits the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    protected abstract TResult VisitUser(User user);

    /// <summary>
    /// Visits the view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <returns></returns>
    protected abstract TResult VisitView(View view);

    /// <summary>
    /// Visits the view column.
    /// </summary>
    /// <param name="viewColumn">The view column.</param>
    /// <returns></returns>
    protected abstract TResult VisitViewColumn(ViewColumn viewColumn);
  }
}