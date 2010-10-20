// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.09

using System;
using Xtensive.Core;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql
{
  /// <summary>
  /// A factory for SQL DDL operations.
  /// </summary>
  public static class SqlDdl
  {
    public static SqlCreateAssertion Create(Assertion assertion)
    {
      ArgumentValidator.EnsureArgumentNotNull(assertion, "assertion");
      return new SqlCreateAssertion(assertion);
    }

    public static SqlCreateCharacterSet Create(CharacterSet characterSet)
    {
      ArgumentValidator.EnsureArgumentNotNull(characterSet, "characterSet");
      return new SqlCreateCharacterSet(characterSet);
    }

    public static SqlCreateCollation Create(Collation collation)
    {
      ArgumentValidator.EnsureArgumentNotNull(collation, "collation");
      return new SqlCreateCollation(collation);
    }

    public static SqlCreateDomain Create(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new SqlCreateDomain(domain);
    }

    public static SqlCreateIndex Create(Index index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      return new SqlCreateIndex(index);
    }

    public static SqlCreatePartitionFunction Create(PartitionFunction partitionFunction)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionFunction, "partitionFunction");
      return new SqlCreatePartitionFunction(partitionFunction);
    }

    public static SqlCreatePartitionScheme Create(PartitionSchema partitionSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlCreatePartitionScheme(partitionSchema);
    }

    public static SqlCreateSchema Create(Schema schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      return new SqlCreateSchema(schema);
    }

    public static SqlCreateSequence Create(Sequence sequence)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlCreateSequence(sequence);
    }

    public static SqlCreateTable Create(Table table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlCreateTable(table);
    }

    public static SqlCreateTranslation Create(Translation translation)
    {
      ArgumentValidator.EnsureArgumentNotNull(translation, "translation");
      return new SqlCreateTranslation(translation);
    }

    public static SqlCreateView Create(View view)
    {
      ArgumentValidator.EnsureArgumentNotNull(view, "view");
      return new SqlCreateView(view);
    }

    public static SqlDropAssertion Drop(Assertion assertion)
    {
      ArgumentValidator.EnsureArgumentNotNull(assertion, "assertion");
      return new SqlDropAssertion(assertion);
    }

    public static SqlDropCharacterSet Drop(CharacterSet characterSet)
    {
      ArgumentValidator.EnsureArgumentNotNull(characterSet, "characterSet");
      return new SqlDropCharacterSet(characterSet);
    }

    public static SqlDropCollation Drop(Collation collation)
    {
      ArgumentValidator.EnsureArgumentNotNull(collation, "collation");
      return new SqlDropCollation(collation);
    }

    public static SqlDropDomain Drop(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new SqlDropDomain(domain);
    }

    public static SqlDropIndex Drop(Index index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      return new SqlDropIndex(index);
    }

    //public static SqlDropIndex Drop(IIndex index, bool? online)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, null);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, null, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, null, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, null, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, null, tableSpace);
    //}

    public static SqlDropPartitionFunction Drop(PartitionFunction partitionFunction)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionFunction, "partitionFunction");
      return new SqlDropPartitionFunction(partitionFunction);
    }

    public static SqlDropPartitionScheme Drop(PartitionSchema partitionSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlDropPartitionScheme(partitionSchema);
    }

    public static SqlDropDomain Drop(Domain domain, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new SqlDropDomain(domain, cascade);
    }

    public static SqlDropSchema Drop(Schema schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      return new SqlDropSchema(schema);
    }

    public static SqlDropSchema Drop(Schema schema, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      return new SqlDropSchema(schema, cascade);
    }

    public static SqlDropSequence Drop(Sequence sequence)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlDropSequence(sequence);
    }

    public static SqlDropSequence Drop(Sequence sequence, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlDropSequence(sequence, cascade);
    }

    public static SqlDropTable Drop(Table table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlDropTable(table);
    }

    public static SqlDropTable Drop(Table table, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlDropTable(table, cascade);
    }

    public static SqlDropTranslation Drop(Translation translation)
    {
      ArgumentValidator.EnsureArgumentNotNull(translation, "translation");
      return new SqlDropTranslation(translation);
    }

    public static SqlDropView Drop(View view)
    {
      ArgumentValidator.EnsureArgumentNotNull(view, "view");
      return new SqlDropView(view);
    }

    public static SqlDropView Drop(View view, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(view, "view");
      return new SqlDropView(view, cascade);
    }

    public static SqlAlterTable Alter(Table table, SqlAction action)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      if (action is SqlSetDefault && ((SqlSetDefault)action).Column==null ||
          action is SqlDropDefault && ((SqlDropDefault)action).Column==null)
        throw new ArgumentException(Strings.ExInvalidActionType, "action");
      if (action is SqlAddColumn && ((SqlAddColumn)action).Column.DataTable!=null &&
          ((SqlAddColumn)action).Column.DataTable!=table ||
          action is SqlSetDefault && ((SqlSetDefault)action).Column.DataTable!=null &&
          ((SqlSetDefault)action).Column.DataTable!=table ||
          action is SqlDropDefault && ((SqlDropDefault)action).Column.DataTable!=null &&
          ((SqlDropDefault)action).Column.DataTable!=table ||
          action is SqlDropColumn && ((SqlDropColumn)action).Column.DataTable!=null &&
          ((SqlDropColumn)action).Column.DataTable!=table ||
          action is SqlAlterIdentityInfo && ((SqlAlterIdentityInfo)action).Column.DataTable!=null &&
          ((SqlAlterIdentityInfo)action).Column.DataTable!=table)
        throw new ArgumentException(Strings.ExColumnBelongsToOtherTable, "action");
      else if (action is SqlAddConstraint) {
        var constraint = ((SqlAddConstraint) action).Constraint as TableConstraint;
        if (constraint==null)
          throw new ArgumentException(Strings.ExInvalidConstraintType, "action");
        else if (constraint.Table!=null && constraint.Table!=table)
          throw new ArgumentException(Strings.ExConstraintBelongsToOtherTable, "action");
      }
      else if (action is SqlDropConstraint) {
        var constraint = ((SqlDropConstraint) action).Constraint as TableConstraint;
        if (constraint==null)
          throw new ArgumentException(Strings.ExInvalidConstraintType, "action");
        else if (constraint.Table!=null && constraint.Table!=table)
          throw new ArgumentException(Strings.ExConstraintBelongsToOtherTable, "action");
      }
      return new SqlAlterTable(table, action);
    }

    public static SqlRenameTable Rename(Table table, string newName)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      if (table.Name==newName)
        throw new ArgumentException(Strings.ExTableAlreadyHasSpecifiedName);
      return new SqlRenameTable(table, newName);
    }

    public static SqlAlterTable Rename(TableColumn column, string newName)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "table");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      if (column.Name==newName)
        throw new ArgumentException(Strings.ExColumnAlreadyHasSpecifiedName);
      return Alter(column.Table, new SqlRenameColumn(column, newName));
    }

    public static SqlAlterSequence Alter(Sequence sequence, SequenceDescriptor descriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterSequence(sequence, descriptor, infoOption);
    }

    public static SqlAlterSequence Alter(Sequence sequence, SequenceDescriptor descriptor)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterSequence(sequence, descriptor, SqlAlterIdentityInfoOptions.All);
    }

    public static SqlAlterDomain Alter(Domain domain, SqlAction action)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      if (action is SqlAddConstraint) {
        DomainConstraint constraint = ((SqlAddConstraint)action).Constraint as DomainConstraint;
        if (constraint==null)
          throw new ArgumentException(Strings.ExInvalidConstraintType, "action");
        else if (constraint.Domain!=null && constraint.Domain!=domain)
          throw new ArgumentException(Strings.ExConstraintBelongsToOtherDomain, "action");
      }
      else if (action is SqlDropConstraint) {
        DomainConstraint constraint = ((SqlDropConstraint)action).Constraint as DomainConstraint;
        if (constraint==null)
          throw new ArgumentException(Strings.ExInvalidConstraintType, "action");
        else if (constraint.Domain!=null && constraint.Domain!=domain)
          throw new ArgumentException(Strings.ExConstraintBelongsToOtherDomain, "action");
      }
      else if (action is SqlSetDefault && ((SqlSetDefault)action).Column!=null ||
               action is SqlDropDefault && ((SqlDropDefault)action).Column!=null)
        throw new ArgumentException(Strings.ExInvalidActionType, "action");
      else if (action is SqlAddColumn || action is SqlDropColumn || action is SqlAlterIdentityInfo)
        throw new ArgumentException(Strings.ExInvalidActionType, "action");
      return new SqlAlterDomain(domain, action);
    }

    public static SqlAlterPartitionFunction Alter(
      PartitionFunction partitionFunction, string booundary, SqlAlterPartitionFunctionOption option)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionFunction, "partitionFunction");
      ArgumentValidator.EnsureArgumentNotNull(booundary, "booundary");
      return new SqlAlterPartitionFunction(partitionFunction, booundary, option);
    }

    public static SqlAlterPartitionScheme Alter(PartitionSchema partitionSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlAlterPartitionScheme(partitionSchema, null);
    }

    public static SqlAlterPartitionScheme Alter(PartitionSchema partitionSchema, string filegroup)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlAlterPartitionScheme(partitionSchema, filegroup);
    }

    public static SqlAlterIdentityInfo Alter(TableColumn column, SequenceDescriptor descriptor)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterIdentityInfo(column, descriptor, SqlAlterIdentityInfoOptions.All);
    }

    public static SqlAlterIdentityInfo Alter(
      TableColumn column, SequenceDescriptor descriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterIdentityInfo(column, descriptor, infoOption);
    }

    public static SqlAddColumn AddColumn(TableColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlAddColumn(column);
    }

    public static SqlAddConstraint AddConstraint(Constraint constraint)
    {
      ArgumentValidator.EnsureArgumentNotNull(constraint, "constraint");
      return new SqlAddConstraint(constraint);
    }

    public static SqlDropColumn DropColumn(TableColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlDropColumn(column);
    }

    public static SqlDropColumn DropColumn(TableColumn column, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlDropColumn(column, cascade);
    }

    public static SqlDropConstraint DropConstraint(Constraint constraint)
    {
      ArgumentValidator.EnsureArgumentNotNull(constraint, "constraint");
      return new SqlDropConstraint(constraint);
    }

    public static SqlDropConstraint DropConstraint(Constraint constraint, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(constraint, "constraint");
      return new SqlDropConstraint(constraint, cascade);
    }

    public static SqlSetDefault SetDefault(SqlExpression defaulValue, TableColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(defaulValue, "defaulValue");
      return new SqlSetDefault(defaulValue, column);
    }

    public static SqlSetDefault SetDefault(SqlExpression defaulValue)
    {
      return SetDefault(defaulValue, null);
    }

    public static SqlDropDefault DropDefault(TableColumn column)
    {
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