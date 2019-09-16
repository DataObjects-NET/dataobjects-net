using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using QueryParameterBinding = Xtensive.Orm.Services.QueryParameterBinding;

namespace Xtensive.Orm.BulkOperations
{
  internal class BulkUpdateOperation<T> : QueryOperation<T>
    where T : class, IEntity
  {
    private readonly SetOperation<T> setOperation;

    #region Non-public methods

    protected override int ExecuteInternal()
    {
      base.ExecuteInternal();
      QueryTranslationResult request = GetRequest(query);
      Bindings = request.ParameterBindings.ToList();
      if (PrimaryIndexes.Length > 1)
        throw new NotImplementedException("Inheritance is not implemented");
      SqlUpdate update = SqlDml.Update(SqlDml.TableRef(PrimaryIndexes[0].Table));
      setOperation.Statement = SetStatement.Create(update);
      Join(update, (SqlSelect) request.Query);
      setOperation.AddValues();
      QueryCommand command = ToCommand(update);
      return command.ExecuteNonQuery();
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

    protected override void SetStatementWhere(SqlStatement statement, SqlExpression @where)
    {
      var update = (SqlUpdate) statement;
      update.Where = where;
    }

    protected override void SetStatementLimit(SqlStatement statement, SqlExpression limit)
    {
      var update = (SqlUpdate) statement;
      update.Limit = limit;
    }

    protected override bool SupportsJoin()
    {
      return DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.UpdateFrom);
    }

    protected override bool SupportsLimitation()
    {
      return DomainHandler.Domain.StorageProviderInfo.Supports(ProviderFeatures.UpdateLimit);
    }


    #endregion

    public BulkUpdateOperation(IQueryable<T> query, List<SetDescriptor> descriptors)
      : base((QueryProvider) query.Provider)
    {
      this.query = query;
      setOperation = new SetOperation<T>(this, descriptors);
    }

    public BulkUpdateOperation(IQueryable<T> query, Expression<Func<T, T>> evaluator)
      : base((QueryProvider) query.Provider)
    {
      this.query = query;
      int memberInitCount = 0;
      ParameterExpression parameter = evaluator.Parameters[0];
      List<SetDescriptor> descriptors = null;
      evaluator.Visit(
        delegate(MemberInitExpression ex) {
          if (memberInitCount > 0)
            return ex;
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
        Expression ex = lambda.Body;
        var ex1 = lambda.Body as UnaryExpression;
        if (ex1!=null && ex1.NodeType==ExpressionType.Convert)
          ex = ex1.Operand;
        var member = (PropertyInfo) ((MemberExpression) ex).Member;
        lambda = (LambdaExpression) expression.Item2;
        var propertyInfo = TypeInfo.Fields.FirstOrDefault(a => a.UnderlyingProperty==member);
        if (propertyInfo==null) {
          if (member.ReflectedType.IsAssignableFrom(TypeInfo.UnderlyingType)) {
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