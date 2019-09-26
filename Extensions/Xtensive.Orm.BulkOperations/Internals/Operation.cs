using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Sql.Model;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using QueryParameterBinding = Xtensive.Orm.Services.QueryParameterBinding;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.BulkOperations
{
  internal abstract class Operation<T>
    where T : class, IEntity
  {
    private static readonly MethodInfo TranslateQueryMethod = typeof(QueryBuilder).GetMethod("TranslateQuery");
    public readonly QueryProvider QueryProvider;
    public List<QueryParameterBinding> Bindings;
    protected DomainHandler DomainHandler;
    protected PrimaryIndexMapping[] PrimaryIndexes;
    public QueryBuilder QueryBuilder;
    public Session Session;
    protected TypeInfo TypeInfo;
    public SqlTableRef JoinedTableRef;

    public int Execute()
    {
      EnsureTransactionIsStarted();
      QueryProvider.Session.SaveChanges();
      int value = ExecuteInternal();
      SessionStateAccessor accessor = DirectStateAccessor.Get(QueryProvider.Session);
      accessor.Invalidate();
      return value;
    }

    #region Non-public methods

    protected void EnsureTransactionIsStarted()
    {
      var accessor = QueryProvider.Session.Services.Demand<DirectSqlAccessor>();
#pragma warning disable 168
      DbTransaction notUsed = accessor.Transaction;
#pragma warning restore 168
    }

    protected abstract int ExecuteInternal();

    public QueryTranslationResult GetRequest(IQueryable<T> query)
    {
      return QueryBuilder.TranslateQuery(query);
    }

    public QueryTranslationResult GetRequest(Type type, IQueryable query)
    {
      return
        (QueryTranslationResult) TranslateQueryMethod.MakeGenericMethod(type).Invoke(QueryBuilder, new object[] {query});
    }

    public TypeInfo GetTypeInfo(Type entityType)
    {
      return Session.Domain.Model.Hierarchies.SelectMany(a => a.Types).Single(a => a.UnderlyingType==entityType);
    }

    #endregion

    protected Operation(QueryProvider queryProvider)
    {
      QueryProvider = queryProvider;
      Type entityType = typeof (T);
      Session = queryProvider.Session;
      DomainHandler = Session.Domain.Services.Get<DomainHandler>();
      TypeInfo =
        queryProvider.Session.Domain.Model.Hierarchies.SelectMany(a => a.Types).Single(
          a => a.UnderlyingType==entityType);
      var mapping = Session.StorageNode.Mapping;
      PrimaryIndexes = TypeInfo.AffectedIndexes
        .Where(i => i.IsPrimary)
        .Select(i => new PrimaryIndexMapping(i, mapping[i.ReflectedType]))
        .ToArray();
      QueryBuilder = Session.Services.Get<QueryBuilder>();
    }

    protected QueryCommand ToCommand(SqlStatement statement)
    {
      return
        QueryBuilder.CreateCommand(
          QueryBuilder.CreateRequest(QueryBuilder.CompileQuery((ISqlCompileUnit) statement), Bindings));
    }
  }
}