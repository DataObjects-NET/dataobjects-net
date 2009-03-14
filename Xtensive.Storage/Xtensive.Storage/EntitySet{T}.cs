// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Activator=System.Activator;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage
{
  public class EntitySet<TItem> : EntitySetBase,
    ICollection<TItem>, IOrderedQueryable<TItem>
    where TItem : Entity
  {
    private static readonly QueryProvider provider = new QueryProvider();
    private Expression expression;
    private Query<TItem> query;

    /// <inheritdoc/>
    [Infrastructure]
    public bool Contains(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return base.Contains(item);
    }

    /// <summary>
    /// Adds the specified element to the <see cref="EntitySet{TItem}"/>.
    /// </summary>
    /// <param name="item">Item to add to the set.</param>
    /// <returns><see langword="True"/> if the element is added to the <see cref="EntitySet{TItem}"/> object; otherwise, <see langword="false"/>.</returns>
    [Infrastructure]
    public bool Add(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Add(item, true);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public bool Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Remove(item, true);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public void Clear()
    {
      Clear(true);
    }

    #region Explicit ICollection<T> members

    /// <inheritdoc/>
    [Infrastructure]
    void ICollection<TItem>.Add(TItem item)
    {
      Add(item);
    }

    /// <inheritdoc/>
    [Infrastructure]
    int ICollection<TItem>.Count
    {
      [DebuggerStepThrough]
      get { return checked ((int) Count); }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public IEnumerator<TItem> GetEnumerator()
    {
//      bool isCached = State.IsFullyLoaded;
//      if (!isCached) {
//        return query.GetEnumerator();
//      }
//      else
//        return GetKeys().Select(key => key.Resolve<TItem>(Session)).GetEnumerator();
      /*long version = State.Version;
            bool isCached = State.IsFullyLoaded;
            IEnumerable<Key> keys = isCached ? State : FetchKeys();

            foreach (Key key in keys) {
              EnsureVersionIs(version);
              if (!isCached)
                State.Register(key);
              yield return key;
            }*/

      foreach (Key key in GetKeys())
        yield return key.Resolve<TItem>();
    }

    

    /// <inheritdoc/>
    [Infrastructure]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    [Infrastructure]
    public void CopyTo(TItem[] array, int arrayIndex)
    {
      foreach (TItem item in this)
        array[arrayIndex++] = item;
    }

    #endregion

    #region IQueryable<T> members

    /// <summary>
    /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
    /// </summary>
    /// <value></value>
    /// <inheritdoc/>
    public Expression Expression
    {
      get  { return expression; }
    }

    /// <inheritdoc/>
    public Type ElementType
    {
      get { return typeof (TItem); }
    }

    /// <inheritdoc/>
    public IQueryProvider Provider
    {
      get { return provider; }
    }

    #endregion

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      if (Field.Association.Multiplicity==Multiplicity.OneToMany) {
        ParameterExpression pe = Expression.Parameter(ElementType, "p");
        Expression l = Expression.Property(pe, Field.Association.Reversed.ReferencingField.Name);
        Expression left = Expression.Property(l, "Key");

        Expression right = Expression.PropertyOrField(Expression.Constant(Owner, typeof (Entity)), "Key");
        Expression e1 = Expression.Equal(left, right);
        expression = Expression.Call(typeof (Queryable), "Where", new[] {ElementType},
          Expression.Constant(this), Expression.Lambda(e1, new[] {pe}));
      }
      else {
        var tmpType = Field.Association.Master.UnderlyingType.UnderlyingType;
        string master = "Master";
        string slave = "Slave";

        if (Field.ReflectedType.UnderlyingType!=Field.Association.Master.ReferencedType.UnderlyingType) {
          var s = master;
          master = slave;
          slave = s;
        }

        ParameterExpression pe = Expression.Parameter(tmpType, "t");
        Expression l = Expression.Property(pe, master);
        Expression left = Expression.Property(l, "Key");
        Expression right = Expression.PropertyOrField(Expression.Constant(Owner, typeof (Entity)), "Key");
        Expression e1 = Expression.Equal(left, right);

        var type = typeof (Query<>);
        var qType = type.MakeGenericType(new[] {tmpType})
          .InvokeMember("All", BindingFlags.Default | BindingFlags.GetProperty, null, null, null);

        Expression where = Expression.Call(typeof (Queryable), "Where", new[] {tmpType},
          Expression.Constant(qType), Expression.Lambda(e1, new[] {pe}));

        ParameterExpression param1 = Expression.Parameter(tmpType, "s");
        var outerSelectorLambda = Expression.Lambda(Expression.Property(Expression.Property(param1, slave), "Key"), param1);
        ParameterExpression param2 = Expression.Parameter(ElementType, "m");
        var innerSelectorLambda = Expression.Lambda(Expression.Property(param2, "Key"), param2);
        var resultsSelectorLambda = Expression.Lambda(param2, param1, param2);

        expression = Expression.Call(typeof (Queryable), "Join", new[]
          {
            tmpType,
            ElementType,
            outerSelectorLambda.Body.Type,
            resultsSelectorLambda.Body.Type
          },
          where, Expression.Constant(this), outerSelectorLambda,
          innerSelectorLambda, resultsSelectorLambda);
      }
      query = new Query<TItem>(expression);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySet(Entity owner, FieldInfo field)
      : base(owner, field, true)
    {
    }
  }
}