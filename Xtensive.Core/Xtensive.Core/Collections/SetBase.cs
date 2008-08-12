using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Comparison;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Base type for sets.
  /// </summary>
  /// <typeparam name="TItem">Type of values to store to set.</typeparam>
  [Serializable]
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
    [DebuggerStepThrough]
    protected bool ContainsNull
    {
      get { return containsNull; }
      set { containsNull = value; }
    }

    #region ISet<TItem> members

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public int Count
    {
      get { return Items.Count + (ContainsNull ? 1 : 0); }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    long ICountable.Count
    {
      get {return Count;}
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
    [DebuggerStepThrough]
    public IEqualityComparer<TItem> Comparer
    {
      get { return comparer; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public virtual bool IsReadOnly
    {
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
    public virtual int RemoveWhere(Predicate<TItem> match)
    {
      ArgumentValidator.EnsureArgumentNotNull(match, "match");
      this.EnsureNotLocked();
      List<TItem> list = new List<TItem>();
      foreach (TItem item in this)
        if (match(item))
          list.Add(item);
      int count = list.Count;
      if (count > 0)
        this.ExceptWith(list);
      return count;
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