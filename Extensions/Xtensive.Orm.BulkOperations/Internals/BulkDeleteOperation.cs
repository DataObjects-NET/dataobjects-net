// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal class BulkDeleteOperation<T> : QueryOperation<T>
    where T : class, IEntity
  {
    #region Non-public methods

    protected override int ExecuteInternal()
    {
      if (PrimaryIndexes.Length > 1) {
        throw new NotImplementedException("Inheritance is not implemented");
      }

      _ = base.ExecuteInternal();

      var request = GetRequest(query);
      Bindings = request.ParameterBindings.ToList();

      using var command = CreateCommand(request);
      return command.ExecuteNonQuery();
    }

    protected async override Task<int> ExecuteInternalAsync(CancellationToken token = default)
    {
      if (PrimaryIndexes.Length > 1) {
        throw new NotImplementedException("Inheritance is not implemented");
      }

      _ = base.ExecuteInternal();

      var request = GetRequest(query);
      Bindings = request.ParameterBindings.ToList();

      var command = CreateCommand(request);
      await using (command.ConfigureAwait(false)) {
        return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
      }
    }

    private QueryCommand CreateCommand(in QueryTranslationResult request)
    {
      var delete = SqlDml.Delete(SqlDml.TableRef(PrimaryIndexes[0].Table));
      Join(delete, request.Query);
      return ToCommand(delete);
    }

    protected override SqlTableRef GetStatementTable(SqlStatement statement)
    {
      var delete = (SqlDelete) statement;
      return delete.Delete;
    }

    protected override SqlExpression GetStatementWhere(SqlStatement statement)
    {
      var delete = (SqlDelete) statement;
      return delete.Where;
    }

    protected override void SetStatementFrom(SqlStatement statement, SqlTable from)
    {
      var delete = (SqlDelete) statement;
      delete.From = from;
    }

    protected override void SetStatementTable(SqlStatement statement, SqlTableRef table)
    {
      var delete = (SqlDelete) statement;
      delete.Delete = table;
    }

    protected override void SetStatementWhere(SqlStatement statement, SqlExpression @where)
    {
      var delete = (SqlDelete) statement;
      delete.Where = where;
    }

    protected override void SetStatementLimit(SqlStatement statement, SqlExpression limit)
    {
      var delete = (SqlDelete) statement;
      delete.Limit = limit;
    }

    protected override bool SupportsJoin() =>
      DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.DeleteFrom);

    protected override bool SupportsLimitation() =>
      DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.DeleteLimit);

    #endregion

    public BulkDeleteOperation(IQueryable<T> query)
      : base((QueryProvider) query.Provider)
    {
      this.query = query;
    }
  }
}
