// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public class EntitySet<TItem> : EntitySetBase,
    ICollection<TItem>
    where TItem : Entity
  {
    /// <inheritdoc/>
    public bool Contains(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return base.Contains(item);
    }

    /// <inheritdoc/>
    public virtual bool Add(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Add(item, true);
    }

    /// <inheritdoc/>
    public virtual bool Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Remove(item, true);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      Clear(true);
    }

    #region Explicit ICollection<T> members

    /// <inheritdoc/>
    void ICollection<TItem>.Add(TItem item)
    {
      Add(item);
    }

    /// <inheritdoc/>
    int ICollection<TItem>.Count
    {
      [DebuggerStepThrough]
      get { return checked ((int) Count); }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      foreach (Key key in GetKeys())
        yield return key.Resolve<TItem>();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
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
    /// <param name="notify">If set to <see langword="true"/>, 
    /// initialization related events will be raised.</param>
    public EntitySet(Persistent owner, FieldInfo field, bool notify)
      : base(owner, field, notify)
    {
    }
  }
}