// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  public partial class SqlCompiler
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitApply(ApplyProvider provider)
    {
      bool processViaCrossApply;
      switch (provider.SequenceType) {
      case ApplySequenceType.All:
        // apply is required
        if (!providerInfo.Supports(ProviderFeatures.Apply))
          throw new NotSupportedException();
        processViaCrossApply = true;
        break;
      case ApplySequenceType.First:
      case ApplySequenceType.FirstOrDefault:
        // apply is prefered but is not required
        processViaCrossApply = providerInfo.Supports(ProviderFeatures.Apply);
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
      bool shouldUseQueryReference;
      var sourceSelect = left.Request.Statement;

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
          || left.Header.Columns.Count!=left.Request.Statement.Columns.Count;
      }

      var binding = OuterReferences.Add(
        provider.ApplyParameter, new Pair<SqlProvider, bool>(left, shouldUseQueryReference));

      using (binding) {
        var right = Compile(provider.Right);

        var query = processViaCrossApply
          ? ProcessApplyViaCrossApply(provider, left, right)
          : ProcessApplyViaSubqueries(provider, left, right, shouldUseQueryReference);

        query.Comment = right.Request.Statement.Comment ?? left.Request.Statement.Comment;
        
        return CreateProvider(query, provider, left, right);
      }
    }

    private SqlSelect ProcessApplyViaSubqueries(ApplyProvider provider, SqlProvider left, SqlProvider right, bool shouldUseQueryReference)
    {
      var rightQuery = right.Request.Statement;
      SqlSelect query;
      if (shouldUseQueryReference) {
        var leftTable = left.PermanentReference;
        query = SqlDml.Select(leftTable);
        query.Columns.AddRange(leftTable.Columns);
      }
      else {
        query = left.Request.Statement.ShallowClone();
      }

      var isApplyExistence =
        provider.Right.Type==ProviderType.Existence ||
        provider.Right.Type==ProviderType.Select && provider.Right.Sources[0].Type==ProviderType.Existence;

      if (isApplyExistence) {
        for (int i = 0; i < rightQuery.Columns.Count; i++) {
          var column = rightQuery.Columns[i];
          if (provider.IsInlined) {
            var columnStub = SqlDml.ColumnStub(column);
            var userColumn = ExtractUserColumn(column);
            stubColumnMap.Add(columnStub, userColumn.Expression);
            column = columnStub;
          }
          query.Columns.Add(column);
        }
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
        : left.Request.Statement.From;
      var leftColumns = leftShouldUseReference
        ? (IReadOnlyList<SqlColumn>) leftTable.Columns
        : left.Request.Statement.Columns;

      var rightShouldUseReference = ShouldUseQueryReference(provider, right) || forceApplyViaReference;
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.Statement.From;
      var rightColumns = rightShouldUseReference
        ? (IReadOnlyList<SqlColumn>) rightTable.Columns
        : right.Request.Statement.Columns;

      var joinType = provider.ApplyType == JoinType.LeftOuter
        ? SqlJoinType.LeftOuterApply
        : SqlJoinType.CrossApply;

      var joinedTable = SqlDml.Join(
        joinType,
        leftTable,
        rightTable,
        leftColumns,
        rightColumns);

      var query = SqlDml.Select(joinedTable);
      if (!leftShouldUseReference)
        query.Where &= left.Request.Statement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.Statement.Where;
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return query;
    }

  }
}