using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Base type for sets.
  /// </summary>
  /// <typeparam name="TItem">Type of values to store to set.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public abstract class SetBase<TItem> : LockableBase, 
    ISet<TItem>
  {
    private bool containsNull;
    private readonly IEqualityComparer<TItem> comparer;
    
    /// <summary>
    /// Gets the underlying dictionary containing all the set items.
    /// </summary>
    protected abstract IDictionary<TItem, TItem> Items { get; }

    /// <summary>
    /// Gets or sets a value indicating whether a set contains <see langword="null"/> item.
    /// </summary>
    protected bool ContainsNull
    {
      [DebuggerStepThrough]
      get { return containsNull; }
      [DebuggerStepThrough]
      set { containsNull = value; }
    }

    #region ISet<TItem> members

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return Items.Count + (ContainsNull ? 1 : 0); }
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      this.EnsureNotLocked();
      Items.Clear();
      ContainsNull = false;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public virtual TItem this[TItem item] {
      get {
        TItem foundItem;
        if (Items.TryGetValue(item, out foundItem))
          return foundItem;
        else
          return default(TItem); 
      }
    }

    /// <inheritdoc/>
    public IEqualityComparer<TItem> Comparer
    {
      [DebuggerStepThrough]
      get { return comparer; }
    }

    /// <inheritdoc/>
    public virtual bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return IsLocked; }
    }

    /// <inheritdoc/>
    public virtual bool Contains(TItem item)
    {
      if (item==null)
        return ContainsNull;
      return Items.ContainsKey(item);
    }

    /// <inheritdoc/>
    public virtual void CopyTo(TItem[] array, int arrayIndex)
    {
      this.Copy(array, arrayIndex);
    }

    /// <inheritdoc/>
    public virtual bool Add(TItem item)
    {
      this.EnsureNotLocked();
      if (item==null) {
        bool contains = ContainsNull;
        ContainsNull = true;
        return !contains;
      }
      if (Contains(item))
        return false;
      Items.Add(item, item);
      return true;
    }

    /// <inheritdoc/>
    void ICollection<TItem>.Add(TItem value)
    {
      Add(value);
    }

    /// <inheritdoc/>
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      foreach (KeyValuePair<TItem, TItem> pair in Items)
        yield return pair.Value;
    }

    /// <inheritdoc/>
    public virtual bool Remove(TItem item)
    {
      this.EnsureNotLocked();
      if (item==null) {
        if (ContainsNull) {
          ContainsNull = false;
          return true;
        }
        return false;
      }
      return Items.Remove(item);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="comparer">The equality comparer to use.</param>
    protected SetBase(IEqualityComparer<TItem> comparer)
    {
      if (comparer==null)
        this.comparer = EqualityComparer<TItem>.Default;
      else
        this.comparer = comparer;
    }
  }
}