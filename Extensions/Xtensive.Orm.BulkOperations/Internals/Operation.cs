// Copyright (C) 2019-2021 Xtensive LLC.
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
using Xtensive.Reflection;
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
    public readonly QueryBuilder QueryBuilder;
    public List<QueryParameterBinding> Bindings;
    public SqlTableRef JoinedTableRef;

    protected readonly DomainHandler DomainHandler;
    protected readonly PrimaryIndexMapping[] PrimaryIndexes;
    protected readonly TypeInfo TypeInfo;

    public Session Session { get { return QueryBuilder.Session; } }

    public int Execute()
    {
      EnsureTransactionIsStarted();
      Session.SaveChanges();
      int value = ExecuteInternal();
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

    protected void EnsureTransactionIsStarted()
    {
      Transaction.Require(QueryProvider.Session);
#pragma warning disable 168
      // this prepares connection which ensures that connection is opened
      // this is weird way but it is required for some scenarios.
      _ = QueryProvider.Session.Services.Demand<DirectSqlAccessor>().Transaction;
#pragma warning restore 168
    }

    protected abstract int ExecuteInternal();

    protected abstract Task<int> ExecuteInternalAsync(CancellationToken token = default);

    public QueryTranslationResult GetRequest(IQueryable<T> query) => QueryBuilder.TranslateQuery(query);

    public QueryTranslationResult GetRequest(Type type, IQueryable query)
    {
      var translateQueryMethod = WellKnownMembers.TranslateQueryMethod.CachedMakeGenericMethod(type);
      return (QueryTranslationResult) translateQueryMethod.Invoke(QueryBuilder, new object[] {query});
    }

    public TypeInfo GetTypeInfo(Type entityType) =>
      Session.Domain.Model.Hierarchies.SelectMany(a => a.Types).Single(a => a.UnderlyingType == entityType);

    protected QueryCommand ToCommand(SqlStatement statement) =>
      QueryBuilder.CreateCommand(
        QueryBuilder.CreateRequest(QueryBuilder.CompileQuery((ISqlCompileUnit) statement), Bindings));

    protected Operation(QueryProvider queryProvider)
    {
      QueryProvider = queryProvider;
      var entityType = typeof (T);
      var session = queryProvider.Session;
      DomainHandler = session.Domain.Services.Get<DomainHandler>();
      QueryBuilder = session.Services.Get<QueryBuilder>();
      TypeInfo = GetTypeInfo(entityType);
      var mapping = session.StorageNode.Mapping;
      PrimaryIndexes = TypeInfo.AffectedIndexes
        .Where(i => i.IsPrimary)
        .Select(i => new PrimaryIndexMapping(i, mapping[i.ReflectedType]))
        .ToArray();
    }
  }
}
