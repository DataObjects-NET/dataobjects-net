using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Orm.BulkOperations
{
  internal class Updatable<T> : IUpdatable<T> where T : IEntity
  {
    internal readonly List<Tuple<Expression, Expression>> Expressions;
    internal readonly IQueryable<T> Query;

    public Updatable(Updatable<T> updatable, Expression field, Expression update)
    {
      Query = updatable.Query;
      Expressions = new List<Tuple<Expression, Expression>>(updatable.Expressions.Count + 1);
      Expressions.AddRange(updatable.Expressions);
      Expressions.Add(Tuple.Create(field, update));
    }

    public Updatable(IQueryable<T> query, Expression field, Expression update)
    {
      Query = query;
      Expressions = new List<Tuple<Expression, Expression>>(1) {Tuple.Create(field, update)};
    }
  }
}