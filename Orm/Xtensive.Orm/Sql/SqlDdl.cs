// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.09

using System;
using Xtensive.Core;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql
{
  /// <summary>
  /// A factory for SQL DDL operations.
  /// </summary>
  public static class SqlDdl
  {
    public static SqlCreateAssertion Create(Assertion assertion)
    {
      ArgumentNullException.ThrowIfNull(assertion);
      return new SqlCreateAssertion(assertion);
    }

    public static SqlCreateCharacterSet Create(CharacterSet characterSet)
    {
      ArgumentNullException.ThrowIfNull(characterSet);
      return new SqlCreateCharacterSet(characterSet);
    }

    public static SqlCreateCollation Create(Collation collation)
    {
      ArgumentNullException.ThrowIfNull(collation);
      return new SqlCreateCollation(collation);
    }

    public static SqlCreateDomain Create(Domain domain)
    {
      ArgumentNullException.ThrowIfNull(domain);
      return new SqlCreateDomain(domain);
    }

    public static SqlCreateIndex Create(Index index)
    {
      ArgumentNullException.ThrowIfNull(index);
      return new SqlCreateIndex(index);
    }

    public static SqlCreatePartitionFunction Create(PartitionFunction partitionFunction)
    {
      ArgumentNullException.ThrowIfNull(partitionFunction);
      return new SqlCreatePartitionFunction(partitionFunction);
    }

    public static SqlCreatePartitionScheme Create(PartitionSchema partitionSchema)
    {
      ArgumentNullException.ThrowIfNull(partitionSchema);
      return new SqlCreatePartitionScheme(partitionSchema);
    }

    public static SqlCreateSchema Create(Schema schema)
    {
      ArgumentNullException.ThrowIfNull(schema);
      return new SqlCreateSchema(schema);
    }

    public static SqlCreateSequence Create(Sequence sequence)
    {
      ArgumentNullException.ThrowIfNull(sequence);
      return new SqlCreateSequence(sequence);
    }

    public static SqlCreateTable Create(Table table)
    {
      ArgumentNullException.ThrowIfNull(table);
      return new SqlCreateTable(table);
    }

    public static SqlCreateTranslation Create(Translation translation)
    {
      ArgumentNullException.ThrowIfNull(translation);
      return new SqlCreateTranslation(translation);
    }

    public static SqlCreateView Create(View view)
    {
      ArgumentNullException.ThrowIfNull(view);
      return new SqlCreateView(view);
    }

    public static SqlDropAssertion Drop(Assertion assertion)
    {
      ArgumentNullException.ThrowIfNull(assertion);
      return new SqlDropAssertion(assertion);
    }

    public static SqlDropCharacterSet Drop(CharacterSet characterSet)
    {
      ArgumentNullException.ThrowIfNull(characterSet);
      return new SqlDropCharacterSet(characterSet);
    }

    public static SqlDropCollation Drop(Collation collation)
    {
      ArgumentNullException.ThrowIfNull(collation);
      return new SqlDropCollation(collation);
    }

    public static SqlDropDomain Drop(Domain domain)
    {
      ArgumentNullException.ThrowIfNull(domain);
      return new SqlDropDomain(domain);
    }

    public static SqlDropIndex Drop(Index index)
    {
      ArgumentNullException.ThrowIfNull(index);
      return new SqlDropIndex(index);
    }

    //public static SqlDropIndex Drop(IIndex index, bool? online)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, online, null);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism, string tableSpace)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism, string tableSpace)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, online, null, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, string tableSpace)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, online, null, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, null, null, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, string tableSpace)
    //{
    //  ArgumentNullException.ThrowIfNull(index, "index");
    //  return new SqlDropIndex(index, null, null, tableSpace);
    //}

    public static SqlDropPartitionFunction Drop(PartitionFunction partitionFunction)
    {
      ArgumentNullException.ThrowIfNull(partitionFunction);
      return new SqlDropPartitionFunction(partitionFunction);
    }

    public static SqlDropPartitionScheme Drop(PartitionSchema partitionSchema)
    {
      ArgumentNullException.ThrowIfNull(partitionSchema);
      return new SqlDropPartitionScheme(partitionSchema);
    }

    public static SqlDropDomain Drop(Domain domain, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(domain);
      return new SqlDropDomain(domain, cascade);
    }

    public static SqlDropSchema Drop(Schema schema)
    {
      ArgumentNullException.ThrowIfNull(schema);
      return new SqlDropSchema(schema);
    }

    public static SqlDropSchema Drop(Schema schema, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(schema);
      return new SqlDropSchema(schema, cascade);
    }

    public static SqlDropSequence Drop(Sequence sequence)
    {
      ArgumentNullException.ThrowIfNull(sequence);
      return new SqlDropSequence(sequence);
    }

    public static SqlDropSequence Drop(Sequence sequence, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(sequence);
      return new SqlDropSequence(sequence, cascade);
    }

    public static SqlDropTable Drop(Table table)
    {
      ArgumentNullException.ThrowIfNull(table);
      return new SqlDropTable(table);
    }

    public static SqlDropTable Drop(Table table, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(table);
      return new SqlDropTable(table, cascade);
    }

    public static SqlDropTranslation Drop(Translation translation)
    {
      ArgumentNullException.ThrowIfNull(translation);
      return new SqlDropTranslation(translation);
    }

    public static SqlDropView Drop(View view)
    {
      ArgumentNullException.ThrowIfNull(view);
      return new SqlDropView(view);
    }

    public static SqlDropView Drop(View view, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(view);
      return new SqlDropView(view, cascade);
    }

    public static SqlTruncateTable Truncate(Table table)
    {
      ArgumentNullException.ThrowIfNull(table);
      return new SqlTruncateTable(table);
    }

    public static SqlAlterTable Alter(Table table, SqlAction action)
    {
      ArgumentNullException.ThrowIfNull(table);
      ArgumentNullException.ThrowIfNull(action);

      switch (action) {
        case SqlSetDefault setDefaultAction2 when setDefaultAction2.Column is null:
          throw new ArgumentException(Strings.ExInvalidActionType, nameof(action));
        case SqlSetDefault setDefaultAction2 when setDefaultAction2.Column.DataTable!=table:
          throw new ArgumentException(Strings.ExColumnBelongsToOtherTable, nameof(action));
        case SqlDropDefault dropDefaultAction2 when dropDefaultAction2.Column is null:
          throw new ArgumentException(Strings.ExInvalidActionType, nameof(action));
        case SqlDropDefault dropDefaultAction2 when dropDefaultAction2.Column.DataTable!=table:
          throw new ArgumentException(Strings.ExColumnBelongsToOtherTable, nameof(action));
        case SqlAddColumn addColumnAction2 when addColumnAction2.Column.DataTable != table:
          throw new ArgumentException(Strings.ExColumnBelongsToOtherTable, nameof(action));
        case SqlDropColumn dropColumnAction2 when dropColumnAction2.Column.DataTable != table :
          throw new ArgumentException(Strings.ExColumnBelongsToOtherTable, nameof(action));
        case SqlAlterIdentityInfo alterIdentityAction2 when alterIdentityAction2.Column.DataTable != table:
          throw new ArgumentException(Strings.ExColumnBelongsToOtherTable, nameof(action));
        case SqlAddConstraint addConstraint2: {
          if (addConstraint2.Constraint is not TableConstraint tConstraint)
            throw new ArgumentException(Strings.ExInvalidConstraintType, nameof(action));
          else if (tConstraint.Table != null && tConstraint.Table != table)
            throw new ArgumentException(Strings.ExConstraintBelongsToOtherTable, nameof(action));
          break;
        }
        case SqlDropConstraint dropConstraint2: {
          if (dropConstraint2.Constraint is not TableConstraint tConstraint)
            throw new ArgumentException(Strings.ExInvalidConstraintType, nameof(action));
          else if (tConstraint.Table != null && tConstraint.Table != table)
            throw new ArgumentException(Strings.ExConstraintBelongsToOtherTable, nameof(action));
          break;
        }

      }
      return new SqlAlterTable(table, action);
    }

    public static SqlRenameTable Rename(Table table, string newName)
    {
      ArgumentNullException.ThrowIfNull(table);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, nameof(newName));
      if (table.Name==newName)
        throw new ArgumentException(Strings.ExTableAlreadyHasSpecifiedName);
      return new SqlRenameTable(table, newName);
    }

    public static SqlAlterTable Rename(TableColumn column, string newName)
    {
      ArgumentNullException.ThrowIfNull(column);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, nameof(newName));
      if (column.Name==newName)
        throw new ArgumentException(Strings.ExColumnAlreadyHasSpecifiedName);
      return Alter(column.Table, new SqlRenameColumn(column, newName));
    }

    public static SqlAlterSequence Alter(Sequence sequence, SequenceDescriptor descriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      ArgumentNullException.ThrowIfNull(sequence);
      ArgumentNullException.ThrowIfNull(descriptor);
      return new SqlAlterSequence(sequence, descriptor, infoOption);
    }

    public static SqlAlterSequence Alter(Sequence sequence, SequenceDescriptor descriptor)
    {
      ArgumentNullException.ThrowIfNull(sequence);
      ArgumentNullException.ThrowIfNull(descriptor);
      return new SqlAlterSequence(sequence, descriptor, SqlAlterIdentityInfoOptions.All);
    }

    public static SqlAlterDomain Alter(Domain domain, SqlAction action)
    {
      ArgumentNullException.ThrowIfNull(domain);
      ArgumentNullException.ThrowIfNull(action);
      if (action is SqlAddConstraint addConstraint) {
        if (addConstraint.Constraint is not DomainConstraint domainConstraint)
          throw new ArgumentException(Strings.ExInvalidConstraintType, nameof(action));
        else if (domainConstraint.Domain!=null && domainConstraint.Domain!=domain)
          throw new ArgumentException(Strings.ExConstraintBelongsToOtherDomain, nameof(action));
      }
      else if (action is SqlDropConstraint dropConstraint) {
        if (dropConstraint.Constraint is not DomainConstraint domainConstraint)
          throw new ArgumentException(Strings.ExInvalidConstraintType, nameof(action));
        else if (domainConstraint.Domain!=null && domainConstraint.Domain!=domain)
          throw new ArgumentException(Strings.ExConstraintBelongsToOtherDomain, nameof(action));
      }
      else if (action is SqlSetDefault setDefaultAction && setDefaultAction.Column!=null ||
               action is SqlDropDefault dropDefaultAction && dropDefaultAction.Column!=null)
        throw new ArgumentException(Strings.ExInvalidActionType, nameof(action));
      else if (action is SqlAddColumn || action is SqlDropColumn || action is SqlAlterIdentityInfo)
        throw new ArgumentException(Strings.ExInvalidActionType, nameof(action));
      return new SqlAlterDomain(domain, action);
    }

    public static SqlAlterPartitionFunction Alter(
      PartitionFunction partitionFunction, string booundary, SqlAlterPartitionFunctionOption option)
    {
      ArgumentNullException.ThrowIfNull(partitionFunction);
      ArgumentNullException.ThrowIfNull(booundary);
      return new SqlAlterPartitionFunction(partitionFunction, booundary, option);
    }

    public static SqlAlterPartitionScheme Alter(PartitionSchema partitionSchema)
    {
      ArgumentNullException.ThrowIfNull(partitionSchema);
      return new SqlAlterPartitionScheme(partitionSchema, null);
    }

    public static SqlAlterPartitionScheme Alter(PartitionSchema partitionSchema, string filegroup)
    {
      ArgumentNullException.ThrowIfNull(partitionSchema);
      return new SqlAlterPartitionScheme(partitionSchema, filegroup);
    }

    public static SqlAlterIdentityInfo Alter(TableColumn column, SequenceDescriptor descriptor)
    {
      ArgumentNullException.ThrowIfNull(column);
      ArgumentNullException.ThrowIfNull(descriptor);
      return new SqlAlterIdentityInfo(column, descriptor, SqlAlterIdentityInfoOptions.All);
    }

    public static SqlAlterIdentityInfo Alter(
      TableColumn column, SequenceDescriptor descriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      ArgumentNullException.ThrowIfNull(column);
      ArgumentNullException.ThrowIfNull(descriptor);
      return new SqlAlterIdentityInfo(column, descriptor, infoOption);
    }

    public static SqlAddColumn AddColumn(TableColumn column)
    {
      ArgumentNullException.ThrowIfNull(column);
      return new SqlAddColumn(column);
    }

    public static SqlAddConstraint AddConstraint(Constraint constraint)
    {
      ArgumentNullException.ThrowIfNull(constraint);
      return new SqlAddConstraint(constraint);
    }

    public static SqlDropColumn DropColumn(TableColumn column)
    {
      ArgumentNullException.ThrowIfNull(column);
      return new SqlDropColumn(column);
    }

    public static SqlDropColumn DropColumn(TableColumn column, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(column);
      return new SqlDropColumn(column, cascade);
    }

    public static SqlDropConstraint DropConstraint(Constraint constraint)
    {
      ArgumentNullException.ThrowIfNull(constraint);
      return new SqlDropConstraint(constraint);
    }

    public static SqlDropConstraint DropConstraint(Constraint constraint, bool cascade)
    {
      ArgumentNullException.ThrowIfNull(constraint);
      return new SqlDropConstraint(constraint, cascade);
    }

    public static SqlSetDefault SetDefault(SqlExpression defaulValue, TableColumn column)
    {
      ArgumentNullException.ThrowIfNull(defaulValue);
      return new SqlSetDefault(defaulValue, column);
    }

    public static SqlSetDefault SetDefault(SqlExpression defaulValue)
    {
      return SetDefault(defaulValue, null);
    }

    public static SqlDropDefault DropDefault(TableColumn column)
    {
      ArgumentNullException.ThrowIfNull(column);
      return new SqlDropDefault(column);
    }

    public static SqlDropDefault DropDefault()
    {
      return DropDefault(null);
    }

    public static SqlCommand Command(SqlCommandType commandType)
    {
      return new SqlCommand(commandType);
    }
  }
}
