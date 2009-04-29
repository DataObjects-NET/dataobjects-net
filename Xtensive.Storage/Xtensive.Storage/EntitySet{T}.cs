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
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage
{
  public class EntitySet<TItem> : EntitySetBase,
    ICollection<TItem>, IOrderedQueryable<TItem>
    where TItem : Entity
  {

    private const int CacheSize = 10240;
    private const int LoadStateCount = 32;

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
      foreach (var item in GetEntities())
        yield return (TItem)item;
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
      get { return QueryProvider.Instance; }
    }

    #endregion

    /// <inheritdoc/>
    protected sealed override EntitySetState LoadState()
    {
      var state = new EntitySetState(CacheSize);
      long stateCount = 0;
      if (((Entity)Owner).State.PersistenceState != PersistenceState.New) {
        stateCount = count.First().GetValue<long>(0);
        if (stateCount <= LoadStateCount)
          foreach (var item in query)
            state.Register(item.Key);
      }
      state.count = stateCount;
      return state;
    }

    protected sealed override IEnumerable<Entity> GetEntities()
    {
      return State.IsFullyLoaded 
        ? GetCachedEntities() 
        : RetrieveEntities();
    }

    #region Private methods

    private IEnumerable<Entity> GetCachedEntities()
    {
      foreach (Key key in State) {
        EnsureVersionIs(State.Version);
        yield return key.Resolve(); ;
      }
    }

    private IEnumerable<Entity> RetrieveEntities()
    {
      foreach (var item in query) {
        State.Register(item.Key);
        yield return item;
      }
    }

    #endregion

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      // hack to make expression look like regular parameter (ParameterExtractor.IsParameter => true)
      var owner = Expression.Property(Expression.Constant(new {Owner = (Entity)Owner}), "Owner");
      expression = QueryHelper.CreateEntitySetQuery(owner, Field);
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