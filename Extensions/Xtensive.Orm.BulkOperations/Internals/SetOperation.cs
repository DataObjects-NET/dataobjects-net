// Copyright (C) 2019-2020 Xtensive LLC.
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

namespace Xtensive.Orm.BulkOperations
{
  internal class SetOperation<T>
    where T : class, IEntity
  {
    private readonly Operation<T> parent;

    public List<SetDescriptor> Descriptors { get; set; }
    public SetStatement Statement { get; set; }

    #region Non-public methods

    private void PreprocessStructures()
    {
      bool changed = true;
      while (changed)
      {
        changed = false;
        foreach (SetDescriptor setDescriptor in Descriptors.Where(a => a.Field.IsStructure).ToArray())
        {
          var memberInit = setDescriptor.Expression as MemberInitExpression;
          if (memberInit != null)
          {
            changed = true;
            Descriptors.Remove(setDescriptor);
            foreach (MemberAssignment binding in memberInit.Bindings)
            {
              FieldInfo f = setDescriptor.Field.Fields.First(a => a.UnderlyingProperty == binding.Member);
              Descriptors.Add(new SetDescriptor(f, setDescriptor.Parameter, binding.Expression));
            }
          }
          else
            foreach (FieldInfo f in setDescriptor.Field.Fields.Where(a => !a.IsStructure))
            {
              changed = true;
              string name = f.Name;
              if (setDescriptor.Field.IsStructure)
                name = name.Remove(0, setDescriptor.Field.Name.Length + 1);
              Descriptors.Remove(setDescriptor);
              Expression ex = setDescriptor.Expression;
              var call = ex as MethodCallExpression;
              if (call != null && call.Method.DeclaringType == typeof(Queryable) &&
                call.Method.Name is "First" or "FirstOrDefault" or "Single" or "SingleOrDefault")
                throw new NotSupportedException("Subqueries with structures are not supported");
              /*ex = call.Arguments[0];
        ParameterExpression parameter = Expression.Parameter(setDescriptor.Expression.Type, "parameter");
        var list = new List<Model.FieldInfo> {f};
        while (list.Last().Parent != setDescriptor.Field)
            list.Add(f.Parent);
        list.Reverse();
        Expression member = parameter;
        foreach (Model.FieldInfo f2 in list)
            member = Expression.MakeMemberAccess(member, f2.UnderlyingProperty);
        LambdaExpression lambda =
            Expression.Lambda(
                typeof (Func<,>).MakeGenericType(parameter.Type, f.ValueType), member, parameter);
        ex = Expression.Call(
            typeof (Queryable), "Select", new[] {parameter.Type, f.ValueType}, ex, lambda);
        ex = Expression.Call(typeof (Queryable), call.Method.Name, new[] {f.ValueType}, ex);*/
              else {
                //ex = Expression.Convert(ex, typeof(Structure));
                var last = f;
                var fields = new List<FieldInfo> { last };
                while (last.Parent != setDescriptor.Field) {
                  last = f.Parent;
                  fields.Add(last);
                }
                Expression member = ex;
                for (var i = fields.Count; i-- > 0;) {
                  member = Expression.MakeMemberAccess(member, fields[i].UnderlyingProperty);
                }
                ex = member;
              }
              Descriptors.Add(new SetDescriptor(f, setDescriptor.Parameter, ex));
            }
        }
      }
    }
    private void AddComputedStaticExpression(AddValueContext addContext)
    {
      SqlTableColumn column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Column.Name);
      var all = Expression.Call(Expression.Constant(parent.Session.Query), "All", new[] { typeof(T) });
      MethodCallExpression selectExpression = Expression.Call(
        typeof(Queryable),
        "OrderBy",
        addContext.Lambda.Type.GetGenericArguments(),
        all,
        addContext.Lambda);
      QueryTranslationResult request = parent.GetRequest(parent.QueryProvider.CreateQuery<T>(selectExpression));
      var sqlSelect = ((SqlSelect)request.Query);
      SqlExpression ex = sqlSelect.OrderBy[0].Expression;
      var placeholder = ex as SqlPlaceholder;
      if (placeholder == null)
      {
        parent.Bindings.AddRange(request.ParameterBindings);
        addContext.Statement.AddValue(column, ex);
        return;
      }
      //hack for this case
      addContext.Lambda=(LambdaExpression) addContext.Lambda.Visit((Expression e) =>
                                                                     {
                                                                       if (e.Type != typeof (Session))
                                                                         return e;
                                                                       return Expression.Property(addContext.Lambda.Parameters[0], "Session");
                                                                     });
      AddComputedExpression(addContext);
    }

    private void AddComputedExpression(AddValueContext addContext)
    {
      SqlTableColumn column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Column.Name);
      var all = Expression.Call(Expression.Constant(parent.Session.Query), "All", new[] {typeof (T)});
      MethodCallExpression selectExpression = Expression.Call(
        typeof (Queryable),
        "OrderBy",
        addContext.Lambda.Type.GetGenericArguments(),
        all,
        addContext.Lambda);
      QueryTranslationResult request = parent.GetRequest(parent.QueryProvider.CreateQuery<T>(selectExpression));
      var sqlSelect = ((SqlSelect) request.Query);
      SqlExpression ex = sqlSelect.OrderBy[0].Expression;
      parent.Bindings.AddRange(request.ParameterBindings);
      
      if(parent.JoinedTableRef!=null)
        ex.AcceptVisitor(new ComputedExpressionSqlVisitor(sqlSelect.From, parent.JoinedTableRef));

      addContext.Statement.AddValue(column, ex);
    }

    private void AddConstantValue(AddValueContext addContext)
    {
      SqlTableColumn column = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Column.Name);
      SqlExpression value;
      object constant = FastExpression.Lambda(addContext.Lambda.Body).Compile().DynamicInvoke();
      if (constant==null)
        value = SqlDml.Null;
      else {
        QueryParameterBinding binding = parent.QueryBuilder.CreateParameterBinding(constant.GetType(), context => constant);
        parent.Bindings.Add(binding);
        value = binding.ParameterReference;
      }
      addContext.Statement.AddValue(column, value);
    }

    private void AddEntityValue(AddValueContext addContext)
    {
      if (addContext.EntityParamExists)
        throw new NotSupportedException("Expressions with reference to updating entity are not supported");
      var methodCall = addContext.Descriptor.Expression as MethodCallExpression;
      int i;
      if (methodCall!=null) {
        if (methodCall.Method.DeclaringType==typeof (QueryEndpoint) &&
          methodCall.Method.Name is "Single" or "SingleOrDefault") {
          object[] keys;
          if (methodCall.Arguments[0].Type==typeof (Key) || methodCall.Arguments[0].Type.IsSubclassOf(typeof (Key))) {
            var key = (Key) methodCall.Arguments[0].Invoke();
            keys = new object[key.Value.Count];
            for (i = 0; i < keys.Length; i++)
              keys[i] = key.Value.GetValue(i);
          }
          else
            keys = (object[]) methodCall.Arguments[0].Invoke();
          i = -1;
          foreach (ColumnInfo column in addContext.Field.Columns) {
            i++;
            SqlExpression value;
            if (keys[i]==null)
              value = SqlDml.Null;
            else {
              object v = keys[i];
              QueryParameterBinding binding = parent.QueryBuilder.CreateParameterBinding(v.GetType(), context => v);
              parent.Bindings.Add(binding);
              value = binding.ParameterReference;
            }
            SqlTableColumn c = SqlDml.TableColumn(addContext.Statement.Table, column.Name);
            addContext.Statement.AddValue(c, value);
          }
          return;
        }
        if (methodCall.Method.DeclaringType==typeof (Queryable) &&
          methodCall.Method.Name is "Single" or "SingleOrDefault" or "First" or "FirstOrDefault") {
          Expression exp = methodCall.Arguments[0];
          TypeInfo info = parent.GetTypeInfo(addContext.Field.ValueType);
          if (methodCall.Arguments.Count==2)
            exp = Expression.Call(
              typeof (Queryable), "Where", new[] {info.UnderlyingType}, exp, methodCall.Arguments[1]);
          exp = Expression.Call(typeof (Queryable), "Take", new[] {info.UnderlyingType}, exp, Expression.Constant(1));
          i = -1;
          foreach (FieldInfo field in
            info.Key.Fields) {
            i++;
            ParameterExpression p = Expression.Parameter(info.UnderlyingType);
            LambdaExpression lambda =
              FastExpression.Lambda(
                WellKnownMembers.FuncOfTArgTResultType.MakeGenericType(info.UnderlyingType, field.ValueType),
                Expression.MakeMemberAccess(p, field.UnderlyingProperty),
                p);
            IQueryable q =
              ((IQueryProvider)parent.QueryProvider).CreateQuery(
                Expression.Call(typeof (Queryable), "Select", new[] {info.UnderlyingType, field.ValueType}, exp, lambda));
            QueryTranslationResult request = parent.GetRequest(field.ValueType, q);
            parent.Bindings.AddRange(request.ParameterBindings);
            SqlTableColumn c = SqlDml.TableColumn(addContext.Statement.Table, addContext.Field.Columns[i].Name);
            addContext.Statement.AddValue(c, SqlDml.SubQuery((ISqlQueryExpression) request.Query));
          }
          return;
        }
      }
      i = -1;
      var entity = (IEntity) FastExpression.Lambda(addContext.Lambda.Body).Compile().DynamicInvoke();
      foreach (ColumnInfo column in addContext.Field.Columns) {
        i++;
        SqlExpression value;
        if (entity==null)
          value = SqlDml.Null;
        else {
          object v = entity.Key.Value.GetValue(i);
          QueryParameterBinding binding = parent.QueryBuilder.CreateParameterBinding(v.GetType(), context => v);
          parent.Bindings.Add(binding);
          value = binding.ParameterReference;
        }
        SqlTableColumn c = SqlDml.TableColumn(addContext.Statement.Table, column.Name);
        addContext.Statement.AddValue(c, value);
      }
    }

    public void AddValues()
    {
      foreach (SetDescriptor descriptor in Descriptors) {
        var addContext = new AddValueContext {
          Descriptor = descriptor,
          Lambda =
            FastExpression.Lambda(
              WellKnownMembers.FuncOfTArgTResultType.MakeGenericType(typeof (T), descriptor.Expression.Type),
              descriptor.Expression,
              descriptor.Parameter),
          Statement = Statement
        };
        descriptor.Expression.Visit(
          delegate(ParameterExpression p) {
            // ReSharper disable AccessToModifiedClosure
            if (p==descriptor.Parameter)
              // ReSharper restore AccessToModifiedClosure
              addContext.EntityParamExists = true;
            return p;
          });
        addContext.SubqueryExists = descriptor.Expression.IsContainsQuery();
        addContext.Field = descriptor.Field;
        if (addContext.Field.IsEntitySet)
          throw new NotSupportedException("EntitySets are not supported");
        if (addContext.Field.IsEntity) {
          AddEntityValue(addContext);
          continue;
        }
        if(!addContext.EntityParamExists && addContext.SubqueryExists)
          AddComputedStaticExpression(addContext);
        else if (addContext.EntityParamExists || addContext.SubqueryExists)
          AddComputedExpression(addContext);
        else
          AddConstantValue(addContext);
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
