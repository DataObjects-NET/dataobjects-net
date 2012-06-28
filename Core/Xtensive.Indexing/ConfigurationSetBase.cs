// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base class for a configuration described by a set of named configurations 
  /// (<typeparamref name="TItem"/>s) contract.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate" copy="true" /></para>
  /// </remarks>
  /// <typeparam name="TItem">Type of particular named configuration.</typeparam>
  [Serializable]
  public abstract class ConfigurationSetBase<TItem> : ConfigurationBase, 
    IConfigurationSet<TItem>
    where TItem: class
  {
    private readonly List<TItem> items = new List<TItem>();

    /// <summary>
    /// Gets the name of the <paramref name="item"/>.
    /// </summary>
    /// <param name="item">Item to get the name of.</param>
    /// <returns>Name of the item.</returns>
    protected abstract string GetItemName(TItem item);

    /// <summary>
    /// Gets the inner list of the items.
    /// </summary>
    protected IList<TItem> Items
    {
      [DebuggerStepThrough]
      get { return items; }
    }

    #region IConfigurationSet<TItem> Members

    /// <inheritdoc/>
    public long Count
    {
      [DebuggerStepThrough]
      get { return items.Count; }
    }

    /// <inheritdoc/>
    public TItem this[string name] {
      get {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
        for (int i = 0; i < items.Count; i++) {
          if (GetItemName(items[i])==name)
            return items[i];
        }
        return null;
      }
    }

    /// <inheritdoc/>
    public TItem this[int index] {
      get {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, items.Count, "index");
        return items[index];
      }
    }

    /// <inheritdoc/>
    public int IndexOf(TItem item)
    {
      return items.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      string name = GetItemName(item);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "item.Name");
      if (this[name]!=null)
        throw new InvalidOperationException(Resources.Strings.ExCollectionAlreadyContainsItemWithSpecifiedKey);
      items.Add(item);
    }

    /// <inheritdoc/>
    public void Remove(string name)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      TItem item = this[name];
      if (item==null)
        throw new InvalidOperationException(string.Format(Resources.Strings.ExItemWithNameWasNotFound, name));
      items.Remove(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      this.EnsureNotLocked();
      items.Clear();
    }

    #endregion

    #region IEnumerable<...> members

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      foreach (TItem item in items)
        yield return item;
    }

    /// <inheritdoc/>
    IEnumerator<Pair<string, TItem>> IEnumerable<Pair<string, TItem>>.GetEnumerator()
    {
      foreach (TItem item in items)
        yield return new Pair<string, TItem>(GetItemName(item), item);
    }

    #endregion

    /// <inheritdoc/>
    public override void Validate()
    {
      foreach (TItem item in items) {
        IConfiguration iConfiguration = item as IConfiguration;
        if (iConfiguration!=null)
          iConfiguration.Validate();
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (TItem item in items) {
        ILockable lockable = item as ILockable;
        if (lockable!=null)
          lockable.Lock();
      }
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      ConfigurationSetBase<TItem> configurationSet = (ConfigurationSetBase<TItem>) source;
      foreach (TItem item in configurationSet) {
        ICloneable cloneable = item as ICloneable;
        if (cloneable!=null)
          Add((TItem)cloneable.Clone());
        else 
          Add(item);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ConfigurationSetBase()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="item">The item to add.</param>
    protected ConfigurationSetBase(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      Add(item);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="item">The item to add first.</param>
    /// <param name="items">The items to add.</param>
    protected ConfigurationSetBase(TItem item, params TItem[] items)
      : this(item)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      for (int i = 0; i < items.Length; i++)
        Add(items[i]);
    }
  }
}