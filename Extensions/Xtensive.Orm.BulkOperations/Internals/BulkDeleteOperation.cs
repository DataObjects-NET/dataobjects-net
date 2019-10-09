using System;
using System.Linq;
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
      base.ExecuteInternal();
      QueryTranslationResult request = GetRequest(query);
      Bindings = request.ParameterBindings.ToList();
      if (PrimaryIndexes.Length > 1)
        throw new NotImplementedException("Inheritance is not implemented");
      SqlDelete delete = SqlDml.Delete(SqlDml.TableRef(PrimaryIndexes[0].Table));
      Join(delete, (SqlSelect) request.Query);
      QueryCommand command = ToCommand(delete);
      int result = command.ExecuteNonQuery();
      return result;
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

    protected override bool SupportsJoin()
    {
      return DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.DeleteFrom);
    }

    protected override bool SupportsLimitation()
    {
      return DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.DeleteLimit);
    }

    #endregion

    public BulkDeleteOperation(IQueryable<T> query)
      : base((QueryProvider) query.Provider)
    {
      this.query = query;
    }
  }
}