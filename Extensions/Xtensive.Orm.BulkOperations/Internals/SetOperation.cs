// Copyright (C) 2019-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Services;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm.BulkOperations
{
  internal class SetOperation<T>
    where T : class, IEntity
  {
    private static readonly Type KeyType = typeof(Key);
    private static readonly Type QueryEndpointType = typeof(QueryEndpoint);
    private static readonly Type SessionType = typeof(Session);

    private readonly Operation<T> parent;

    public List<SetDescriptor> Descriptors { get; set; }
    public SetStatement Statement { get; set; }

    public void AddValues()
    {
      var valuesDictionary = new Dictionary<SqlColumn, SqlExpression>(Descriptors.Count * 2);
      foreach (var descriptor in Descriptors) {
        if (descriptor.Field.IsEntitySet) {
          throw new NotSupportedException("EntitySets are not supported");
        }

        var addContext = new AddValueContext {
          Descriptor = descriptor,
          Lambda =
            FastExpression.Lambda(
              WellKnownMembers.FuncOfTArgTResultType.CachedMakeGenericType(typeof(T), descriptor.Expression.Type),
              descriptor.Expression,
              descriptor.Parameter),
          Statement = Statement,
          Values = valuesDictionary,
        };
        _ = descriptor.Expression.Visit(
          delegate (ParameterExpression p) {
            // ReSharper disable AccessToModifiedClosure
            if (p == descriptor.Parameter)
              // ReSharper restore AccessToModifiedClosure
              addContext.EntityParamExists = true;
            return p;
          });

        var subqueryExists = descriptor.Expression.IsContainsQuery();

        if (addContext.Field.IsEntity) {
          AddEntityValue(addContext);
          continue;
        }
        if (!addContext.EntityParamExists && subqueryExists)
          AddComputedStaticExpression(addContext);
        else if (addContext.EntityParamExists || subqueryExists)
          AddComputedExpression(addContext);
        else
          AddConstantValue(addContext);
      }
      Statement.AddValues(valuesDictionary);
    }

    #region Non-public methods

    private void PreprocessStructures()
    {
      var changed = true;
      while (changed) {
        changed = false;
        foreach (var setDescriptor in Descriptors.Where(a => a.Field.IsStructure).ToArray()) {
          if (setDescriptor.Expression is MemberInitExpression memberInit) {
            changed = true;
            _ = Descriptors.Remove(setDescriptor);
            foreach (var binding in memberInit.Bindings.Cast<MemberAssignment>()) {
              var field = setDescriptor.Field.Fields.First(a => a.UnderlyingProperty == binding.Member);
              Descriptors.Add(new SetDescriptor(field, setDescriptor.Parameter, binding.Expression));
            }
          }
          else {
            foreach (var field in setDescriptor.Field.Fields.Where(a => !a.IsStructure)) {
              changed = true;
              var name = setDescriptor.Field.IsStructure
                ? field.Name.Remove(0, setDescriptor.Field.Name.Length + 1)
                : field.Name;
              _ = Descriptors.Remove(setDescriptor);
              var exp = setDescriptor.Expression;
              //var call = ex as MethodCallExpression;
              if (exp is MethodCallExpression call && call.Method.DeclaringType == WellKnownMembers.QueryableType
                && call.Method.Name is nameof(Queryable.First) or nameof(Queryable.FirstOrDefault)
                    or nameof(Queryable.Single) or nameof(Queryable.SingleOrDefault)) {
                throw new NotSupportedException("Subqueries with structures are not supported");
              }
              else {
                //ex = Expression.Convert(ex, typeof(Structure));
                var last = field;
                var fields = new List<FieldInfo> { last };
                while (last.Parent != setDescriptor.Field) {
                  last = field.Parent;
                  fields.Add(last);
                }
                var member = exp;
                for (var i = fields.Count; i-- > 0;) {
                  member = Expression.MakeMemberAccess(member, fields[i].UnderlyingProperty);
                }
                exp = member;
              }
              Descriptors.Add(new SetDescriptor(field, setDescriptor.Parameter, exp));
            }
          }
        }
      }
    }
    private void AddComputedStaticExpression(AddValueContext addContext)
    {
      var column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Column.Name);
      var all = Expression.Call(Expression.Constant(parent.Session.Query), nameof(QueryEndpoint.All), new[] { typeof(T) });
      var selectExpression = Expression.Call(
        WellKnownMembers.QueryableType,
        nameof(Queryable.OrderBy),
        addContext.Lambda.Type.GetGenericArguments(),
        all,
        addContext.Lambda);

      var request = parent.GetRequest(parent.QueryProvider.CreateQuery<T>(selectExpression));
      var sqlSelect = request.Query;
      var exp = sqlSelect.OrderBy[0].Expression;

      if (exp is not SqlPlaceholder placeholder) {
        parent.Bindings.AddRange(request.ParameterBindings);
        addContext.Values.Add(column, exp);
        return;
      }
      //hack for this case
      addContext.Lambda = (LambdaExpression) addContext.Lambda
        .Visit((Expression e) => e.Type != SessionType
          ? e
          : Expression.Property(addContext.Lambda.Parameters[0], "Session"));
      AddComputedExpression(addContext);
    }

    private void AddComputedExpression(AddValueContext addContext)
    {
      var column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Column.Name);
      var all = Expression.Call(Expression.Constant(parent.Session.Query), nameof(QueryEndpoint.All), new[] {typeof (T)});
      var selectExpression = Expression.Call(
        WellKnownMembers.QueryableType,
        nameof(Queryable.OrderBy),
        addContext.Lambda.Type.GetGenericArguments(),
        all,
        addContext.Lambda);

      var request = parent.GetRequest(parent.QueryProvider.CreateQuery<T>(selectExpression));
      var sqlSelect = request.Query;
      var exp = sqlSelect.OrderBy[0].Expression;
      parent.Bindings.AddRange(request.ParameterBindings);

      if (parent.JoinedTableRef != null) {
        exp.AcceptVisitor(new ComputedExpressionSqlVisitor(sqlSelect.From, parent.JoinedTableRef));
      }

      addContext.Values.Add(column, exp);
      //addContext.Statement.AddValue(column, ex);
    }

    private void AddConstantValue(AddValueContext addContext)
    {
      var column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Column.Name);
      var constant = addContext.EvalLambdaBody();
      SqlExpression value;
      if (constant == null) {
        value = SqlDml.Null;
      }
      else {
        var binding = parent.QueryBuilder.CreateParameterBinding(constant.GetType(), context => constant);
        parent.Bindings.Add(binding);
        value = binding.ParameterReference;
      }
      addContext.Values.Add(column, value);
      //addContext.Statement.AddValue(column, value);
    }

    private void AddEntityValue(AddValueContext addContext)
    {
      if (addContext.EntityParamExists) {
        throw new NotSupportedException("Expressions with reference to updating entity are not supported");
      }

      //var methodCall = addContext.Descriptor.Expression as MethodCallExpression;
      int i;
      if (addContext.Descriptor.Expression is MethodCallExpression methodCall) {
        if (methodCall.Method.DeclaringType == QueryEndpointType
          && methodCall.Method.Name is nameof(QueryEndpoint.Single) or nameof(QueryEndpoint.SingleOrDefault)) {

          object[] keys;
          if (methodCall.Arguments[0].Type == KeyType || methodCall.Arguments[0].Type.IsSubclassOf(KeyType)) {
            var key = (Key) methodCall.Arguments[0].Invoke();
            keys = new object[key.Value.Count];
            for (i = 0; i < keys.Length; i++) {
              keys[i] = key.Value.GetValue(i);
            }
          }
          else {
            keys = (object[]) methodCall.Arguments[0].Invoke();
          }

          i = -1;
          foreach (var column in addContext.Field.Columns) {
            i++;
            SqlExpression value;
            if (keys[i]==null) {
              value = SqlDml.Null;
            }
            else {
              var v = keys[i];
              var binding = parent.QueryBuilder.CreateParameterBinding(v.GetType(), context => v);
              parent.Bindings.Add(binding);
              value = binding.ParameterReference;
            }
            var c = SqlDml.TableColumn(addContext.Statement.Table, column.Name);
            addContext.Values.Add(c, value);
            //addContext.Statement.AddValue(c, value);
          }
          return;
        }
        if (methodCall.Method.DeclaringType == WellKnownMembers.QueryableType
          && (methodCall.Method.Name is nameof(Queryable.Single) or nameof(Queryable.SingleOrDefault)
                or nameof(Queryable.First) or nameof(Queryable.FirstOrDefault))) {

          var exp = methodCall.Arguments[0];
          var fieldValueType = parent.GetTypeInfo(addContext.Field.ValueType);
          if (methodCall.Arguments.Count == 2) {
            exp = Expression.Call(WellKnownMembers.QueryableType,
              nameof(Queryable.Where), new[] { fieldValueType.UnderlyingType }, exp, methodCall.Arguments[1]);
          }
          exp = Expression.Call(WellKnownMembers.QueryableType, nameof(Queryable.Take), new[] {fieldValueType.UnderlyingType}, exp, Expression.Constant(1));
          i = -1;
          foreach (var field in fieldValueType.Key.Fields) {
            i++;
            var p = Expression.Parameter(fieldValueType.UnderlyingType);
            var lambda =
              FastExpression.Lambda(
                WellKnownMembers.FuncOfTArgTResultType.CachedMakeGenericType(fieldValueType.UnderlyingType, field.ValueType),
                Expression.MakeMemberAccess(p, field.UnderlyingProperty),
                p);
            var q = ((IQueryProvider) parent.QueryProvider)
              .CreateQuery(Expression.Call(
                WellKnownMembers.QueryableType,
                nameof(Queryable.Select),
                new[] { fieldValueType.UnderlyingType, field.ValueType },
                exp,
                lambda));
            var request = parent.GetRequest(field.ValueType, q);
            parent.Bindings.AddRange(request.ParameterBindings);
            var column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Columns[i].Name);
            addContext.Values.Add(column, SqlDml.SubQuery(request.Query));
            //addContext.Statement.AddValue(c, SqlDml.SubQuery(request.Query));
          }
          return;
        }
      }
      i = -1;
      var entity = (IEntity) addContext.EvalLambdaBody();

      foreach (var column in addContext.Field.Columns) {
        i++;
        SqlExpression value;
        if (entity == null) {
          value = SqlDml.Null;
        }
        else {
          var v = entity.Key.Value.GetValue(i);
          var binding = parent.QueryBuilder.CreateParameterBinding(v.GetType(), context => v);
          parent.Bindings.Add(binding);
          value = binding.ParameterReference;
        }
        var c = SqlDml.TableColumn(addContext.Statement.Table, column.Name);
        addContext.Values.Add(c, value);
        //addContext.Statement.AddValue(c, value);
      }
    }

    #endregion

    public SetOperation(Operation<T> parent, List<SetDescriptor> descriptors)
    {
      this.parent = parent;
      Descriptors = descriptors;
      PreprocessStructures();
    }
  }
}
