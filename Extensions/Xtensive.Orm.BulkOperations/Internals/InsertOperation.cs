using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Services;
using Xtensive.Sql;

namespace Xtensive.Orm.BulkOperations
{
  internal class InsertOperation<T> : Operation<T>
    where T : Entity
  {
    private readonly SetOperation<T> setOperation;

    #region Non-public methods

    protected override int ExecuteInternal()
    {
      if (PrimaryIndexes.Length > 1) {
        throw new NotImplementedException("Inheritance is not implemented");
      }
      Bindings = new List<QueryParameterBinding>();

      var command = CreateCommand();
      return command.ExecuteNonQuery();
    }

    protected override async Task<int> ExecuteInternalAsync(CancellationToken token = default)
    {
      if (PrimaryIndexes.Length > 1) {
        throw new NotImplementedException("Inheritance is not implemented");
      }
      Bindings = new List<QueryParameterBinding>();

      var command = CreateCommand();
      return await command.ExecuteNonQueryAsync(token);
    }

    private QueryCommand CreateCommand()
    {
      var insert = SqlDml.Insert(SqlDml.TableRef(PrimaryIndexes[0].Table));
      setOperation.Statement = SetStatement.Create(insert);
      setOperation.AddValues();
      return ToCommand(insert);
    }

    #endregion

    public InsertOperation(QueryProvider queryProvider, Expression<Func<T>> evaluator)
      : base(queryProvider)
    {
      var memberInitCount = 0;
      var parameter = Expression.Parameter(typeof (T));
      List<SetDescriptor> descriptors = null;
      evaluator.Visit(
        delegate(MemberInitExpression ex) {
          if (memberInitCount > 0) {
            return ex;
          }

          memberInitCount++;
          descriptors = new List<SetDescriptor>();
          foreach (var assignment in ex.Bindings.Cast<MemberAssignment>()) {
            var fieldInfo = TypeInfo.Fields.FirstOrDefault(a => a.UnderlyingProperty == assignment.Member);
            if (fieldInfo == null) {
              if (assignment.Member.ReflectedType?.IsAssignableFrom(TypeInfo.UnderlyingType) == true) {
                var property = TypeInfo.UnderlyingType.GetProperty(assignment.Member.Name);
                fieldInfo = TypeInfo.Fields.FirstOrDefault(field => field.UnderlyingProperty == property);
              }
            }

            descriptors.Add(
              new SetDescriptor(fieldInfo, parameter, assignment.Expression));
          }

          return ex;
        });
      AddKey(descriptors);
      setOperation = new SetOperation<T>(this, descriptors);
    }

    private void AddKey(List<SetDescriptor> descriptors)
    {
      var count = descriptors.Count(a => a.Field.IsPrimaryKey);
      int i;
      if (count==0) {
        var key = Key.Generate<T>(Session);
        i = 0;
        foreach (var fieldInfo in TypeInfo.Key.Fields) {
          descriptors.Add(new SetDescriptor(fieldInfo, Expression.Parameter(typeof(T)), Expression.Constant(key.Value.GetValue(i))));
          i++;
        }
        Key = key;
        return;
      }
      if(count<TypeInfo.Key.Fields.Count) {
        throw new InvalidOperationException("You must set 0 or all key fields");
      }
      i = 0;
      var keys = new object[TypeInfo.Key.Fields.Count];
      foreach(var field in TypeInfo.Key.Fields) {
        var descriptor = descriptors.First(a => a.Field.Equals(field));
        keys[i] = descriptor.Expression.Invoke();
        i++;
      }
      Key = Key.Create<T>(Session.Domain, keys);
    }

    public Key Key { get; private set; }
  }
}