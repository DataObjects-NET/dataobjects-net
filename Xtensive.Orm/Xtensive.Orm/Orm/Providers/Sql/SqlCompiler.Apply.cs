// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  partial class SqlCompiler 
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitApply(ApplyProvider provider)
    {
      bool processViaCrossApply;
      switch (provider.SequenceType) {
      case ApplySequenceType.All:
        // apply is required
        if (!ProviderInfo.Supports(ProviderFeatures.Apply))
          throw new NotSupportedException();
        processViaCrossApply = true;
        break;
      case ApplySequenceType.First:
      case ApplySequenceType.FirstOrDefault:
        // apply is prefered but is not required
        processViaCrossApply = ProviderInfo.Supports(ProviderFeatures.Apply);
        break;
      case ApplySequenceType.Single:
      case ApplySequenceType.SingleOrDefault:
        // apply is not allowed
        processViaCrossApply = false;
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }

      var left = Compile(provider.Left);
      var shouldUseQueryReference = true;
      var sourceSelect = left.Request.SelectStatement;

      if (processViaCrossApply) {
        shouldUseQueryReference = ShouldUseQueryReference(provider, left);
      }
      else {
        var calculatedColumnIndexes = sourceSelect.Columns
          .Select((c, i) => IsCalculatedColumn(c) ? i : -1)
          .Where(i => i >= 0)
          .ToList();
        var groupByIsUsed = sourceSelect.GroupBy.Count > 0;

        var usedOuterColumns = new List<int>();
        var visitor = new ApplyParameterAccessVisitor(provider.ApplyParameter, (mc, index) => {
          usedOuterColumns.Add(index);
          return mc;
        });
        var providerVisitor = new CompilableProviderVisitor((p, e) => visitor.Visit(e));
        providerVisitor.VisitCompilable(provider.Right);
        shouldUseQueryReference = usedOuterColumns.Any(calculatedColumnIndexes.Contains) 
          || groupByIsUsed 
          || provider.Left.Type.In(ProviderType.Store, ProviderType.Include)
          || left.Header.Columns.Count != left.Request.SelectStatement.From.Columns.Count;
      }
      if (!shouldUseQueryReference)
        left = new SqlProvider(left, sourceSelect.From);

      using (OuterReferences.Add(provider.ApplyParameter, left)) {
        var right = Compile(provider.Right);

        var query = processViaCrossApply
          ? ProcessApplyViaCrossApply(provider, left, right)
          : ProcessApplyViaSubqueries(provider, left, right, shouldUseQueryReference);

        return CreateProvider(query, provider, left, right);
      }
    }
    private SqlSelect ProcessApplyViaSubqueries(ApplyProvider provider, SqlProvider left, SqlProvider right, bool shouldUseQueryReference)
    {
      var rightQuery = right.Request.SelectStatement;
      SqlSelect query;
      if (shouldUseQueryReference) {
        var leftTable = left.PermanentReference;
        query = SqlDml.Select(leftTable);
        query.Columns.AddRange(leftTable.Columns.Cast<SqlColumn>());
      }
      else
        query = left.Request.SelectStatement.ShallowClone();

      if (provider.Right.Type==ProviderType.Existence) {
        var column = rightQuery.Columns[0];
        if (provider.IsInlined) {
          var columnStub = SqlDml.ColumnStub(column);
          var userColumn = ExtractUserColumn(column);
          stubColumnMap.Add(columnStub, userColumn.Expression);
          column = columnStub;
        }
        query.Columns.Add(column);
      }
      else {
        if (provider.IsInlined) {
          for (int i = 0; i < rightQuery.Columns.Count; i++) {
            var subquery = rightQuery.ShallowClone();
            var sqlColumn = subquery.Columns[i];
            if (IsColumnStub(sqlColumn)) {
              var columnStub = ExtractColumnStub(sqlColumn);
              subquery.Columns.Clear();
              subquery.Columns.Add(columnStub.Column);
              query.Columns.Add(subquery, sqlColumn.Name);
            }
            else {
              var columnRef = (SqlColumnRef) sqlColumn;
              var column = columnRef.SqlColumn;
              subquery.Columns.Clear();
              subquery.Columns.Add(column);
              var columnName = ProcessAliasedName(provider.Right.Header.Columns[i].Name);
              var userColumnRef = SqlDml.ColumnRef(SqlDml.Column(subquery), columnName);
              var columnStub = SqlDml.ColumnStub(userColumnRef);
              stubColumnMap.Add(columnStub, subquery);
              query.Columns.Add(columnStub);
            }
          }
        }
        else
          for (int i = 0; i < rightQuery.Columns.Count; i++) {
            var subquery = rightQuery.ShallowClone();
            var column = subquery.Columns[i];
            if (IsColumnStub(column)) {
              var columnStub = ExtractColumnStub(column);
              subquery.Columns.Clear();
              subquery.Columns.Add(columnStub.Column);
              query.Columns.Add(subquery, column.Name);
            }
            else {
              var columnRef = (SqlColumnRef)column;
              var sqlColumn = columnRef.SqlColumn;
              subquery.Columns.Clear();
              subquery.Columns.Add(sqlColumn);
              query.Columns.Add(subquery, columnRef.Name);
            }
          }
      }
      return query;
    }

    private SqlSelect ProcessApplyViaCrossApply(ApplyProvider provider, SqlProvider left, SqlProvider right)
    {
      var leftShouldUseReference = ShouldUseQueryReference(provider, left);
      var leftTable = leftShouldUseReference
        ? left.PermanentReference
        : left.Request.SelectStatement.From;
      var leftColumns = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlColumn>()
        : left.Request.SelectStatement.Columns;

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.SelectStatement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.SelectStatement.Columns;

      var joinType = provider.ApplyType==JoinType.LeftOuter
        ? SqlJoinType.LeftOuterApply
        : SqlJoinType.CrossApply;

      var joinedTable = SqlDml.Join(
        joinType,
        leftTable,
        rightTable,
        leftColumns.ToList(),
        rightColumns.ToList());

      var query = SqlDml.Select(joinedTable);
      if (!leftShouldUseReference)
        query.Where &= left.Request.SelectStatement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.SelectStatement.Where;
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return query;
    }

  }
}