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
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public class EntitySet<TItem> : EntitySetBase,
    ICollection<TItem>,
    IQueryable<TItem>
    where TItem : Entity
  {
    #region IQueryable<TItem> implementation

    Expression IQueryable.Expression
    {
      get
      {
//        var result = Query<TItem>.All.Where(p => p.);
//        return Expression.Constant(result);
        throw new NotImplementedException();
      }
    }

    Type IQueryable.ElementType
    {
      get { return typeof (TItem); }
    }

    IQueryProvider IQueryable.Provider
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    public EntitySet(Entity owner, FieldInfo field)
      : base(owner, field, true)
    {
      Initialize(true);
    }
  }
}