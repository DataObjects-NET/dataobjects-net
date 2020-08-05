// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using QueryParameterBinding = Xtensive.Orm.Services.QueryParameterBinding;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.BulkOperations
{
  internal abstract class Operation<T>
    where T : class, IEntity
  {
    public readonly QueryProvider QueryProvider;
    public List<QueryParameterBinding> Bindings;
    protected readonly DomainHandler DomainHandler;
    protected readonly PrimaryIndexMapping[] PrimaryIndexes;
    public readonly QueryBuilder QueryBuilder;
    public readonly Session Session;
    protected readonly TypeInfo TypeInfo;
    public SqlTableRef JoinedTableRef;

    public int Execute()
    {
      EnsureTransactionIsStarted();
      QueryProvider.Session.SaveChanges();
      var value = ExecuteInternal();
      DirectStateAccessor.Get(QueryProvider.Session).Invalidate();
      return value;
    }

    public async Task<int> ExecuteAsync(CancellationToken token = default)
    {
      EnsureTransactionIsStarted();
      await QueryProvider.Session.SaveChangesAsync(token).ConfigureAwait(false);
      var value = await ExecuteInternalAsync(token).ConfigureAwait(false);
      DirectStateAccessor.Get(QueryProvider.Session).Invalidate();
      return value;
    }

    private void EnsureTransactionIsStarted()
    {
      var accessor = QueryProvider.Session.Services.Demand<DirectSqlAccessor>();
      _ = accessor.Transaction;
    }

    protected abstract int ExecuteInternal();

    protected abstract Task<int> ExecuteInternalAsync(CancellationToken token = default);

    public QueryTranslationResult GetRequest(IQueryable<T> query) => QueryBuilder.TranslateQuery(query);

    public QueryTranslationResult GetRequest(Type type, IQueryable query)
    {
      var translateQueryMethod = WellKnownMembers.TranslateQueryMethod.MakeGenericMethod(type);
      return (QueryTranslationResult) translateQueryMethod.Invoke(QueryBuilder, new object[] {query});
    }

    public TypeInfo GetTypeInfo(Type entityType) =>
      Session.Domain.Model.Hierarchies.SelectMany(a => a.Types).Single(a => a.UnderlyingType==entityType);

    protected QueryCommand ToCommand(SqlStatement statement) =>
      QueryBuilder.CreateCommand(
        QueryBuilder.CreateRequest(QueryBuilder.CompileQuery((ISqlCompileUnit) statement), Bindings));

    protected Operation(QueryProvider queryProvider)
    {
      QueryProvider = queryProvider;
      var entityType = typeof (T);
      Session = queryProvider.Session;
      DomainHandler = Session.Domain.Services.Get<DomainHandler>();
      TypeInfo = GetTypeInfo(entityType);
      var mapping = Session.StorageNode.Mapping;
      PrimaryIndexes = TypeInfo.AffectedIndexes
        .Where(i => i.IsPrimary)
        .Select(i => new PrimaryIndexMapping(i, mapping[i.ReflectedType]))
        .ToArray();
      QueryBuilder = Session.Services.Get<QueryBuilder>();
    }
  }
}