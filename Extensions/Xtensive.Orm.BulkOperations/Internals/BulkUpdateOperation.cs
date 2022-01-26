// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal class BulkUpdateOperation<T> : QueryOperation<T>
    where T : class, IEntity
  {
    private readonly SetOperation<T> setOperation;

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
      var update = SqlDml.Update(SqlDml.TableRef(PrimaryIndexes[0].Table));
      setOperation.Statement = SetStatement.Create(update);
      Join(update, request.Query);
      setOperation.AddValues();
      return ToCommand(update);
    }

    protected override SqlTableRef GetStatementTable(SqlStatement statement)
    {
      var update = (SqlUpdate) statement;
      return update.Update;
    }

    protected override SqlExpression GetStatementWhere(SqlStatement statement)
    {
      var update = (SqlUpdate) statement;
      return update.Where;
    }

    protected override void SetStatementFrom(SqlStatement statement, SqlTable from)
    {
      var update = (SqlUpdate)statement;
      update.From = from;
    }

    protected override void SetStatementTable(SqlStatement statement, SqlTableRef table)
    {
      var update = (SqlUpdate) statement;
      update.Update = table;
    }

    protected override void SetStatementWhere(SqlStatement statement, SqlExpression where)
    {
      var update = (SqlUpdate) statement;
      update.Where = where;
    }

    protected override void SetStatementLimit(SqlStatement statement, SqlExpression limit)
    {
      var update = (SqlUpdate) statement;
      update.Limit = limit;
    }

    protected override bool SupportsJoin() =>
      DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.UpdateFrom);

    protected override bool SupportsLimitation() =>
      DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.UpdateLimit);

    #endregion

    public BulkUpdateOperation(IQueryable<T> query, Expression<Func<T, T>> evaluator)
      : base((QueryProvider) query.Provider)
    {
      this.query = query;
      var memberInitCount = 0;
      var parameter = evaluator.Parameters[0];
      List<SetDescriptor> descriptors = null;
      evaluator.Visit(
        delegate(MemberInitExpression ex) {
          if (memberInitCount > 0) {
            return ex;
          }

          memberInitCount++;
          descriptors = (from MemberAssignment assigment in ex.Bindings
            select
              new SetDescriptor(
                TypeInfo.Fields.First(a => a.UnderlyingProperty==assigment.Member), parameter, assigment.Expression)).
            ToList();
          return ex;
        });
      setOperation=new SetOperation<T>(this, descriptors);
    }

    public BulkUpdateOperation(IUpdatable<T> query)
      : base((QueryProvider) ((Updatable<T>) query).Query.Provider)
    {
      var descriptors = new List<SetDescriptor>();
      var q = (Updatable<T>) query;
      this.query = q.Query;
      foreach (var expression in q.Expressions) {
        var lambda = (LambdaExpression) expression.Item1;
        var ex = lambda.Body;
        if (lambda.Body is UnaryExpression unaryExpression && unaryExpression.NodeType==ExpressionType.Convert) {
          ex = unaryExpression.Operand;
        }

        var member = (PropertyInfo) ((MemberExpression) ex).Member;
        lambda = (LambdaExpression) expression.Item2;
        var propertyInfo = TypeInfo.Fields.FirstOrDefault(a => a.UnderlyingProperty==member);
        if (propertyInfo==null) {
          if (member.ReflectedType?.IsAssignableFrom(TypeInfo.UnderlyingType)==true) {
            member = TypeInfo.UnderlyingType.GetProperty(member.Name);
            propertyInfo = TypeInfo.Fields.FirstOrDefault(field => field.UnderlyingProperty==member);
          }
        }
        descriptors.Add(
          new SetDescriptor(propertyInfo, lambda.Parameters[0], lambda.Body));
      }
      setOperation=new SetOperation<T>(this, descriptors);
    }
  }
}
