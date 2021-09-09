// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Caching
{
  public abstract class CacheBase<TKey, TItem> : ICache<TKey, TItem>
  {
    /// <inheritdoc/>
    public virtual Converter<TItem, TKey> KeyExtractor { [DebuggerStepThrough]get; protected set; }

    /// <inheritdoc/>
    public virtual TItem this[TKey key, bool markAsHit] => TryGetItem(key, markAsHit, out var item) ? item : default;

    /// <inheritdoc/>
    public abstract int Count { get; }

    /// <inheritdoc/>
    public abstract TItem Add(TItem item, bool replaceIfExists);

    /// <inheritdoc/>
    public virtual void Add(TItem item) => Add(item, true);

    /// <inheritdoc/>
    public virtual void Clear() => throw new NotImplementedException();

    /// <inheritdoc/>
    public virtual bool ContainsKey(TKey key) => throw new NotImplementedException();

    /// <inheritdoc/>
    public virtual IEnumerator<TItem> GetEnumerator() => throw new NotImplementedException();

    /// <inheritdoc/>
    public virtual void Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public abstract void RemoveKey(TKey key);

    /// <inheritdoc/>
    public abstract void RemoveKey(TKey key, bool removeCompletely);

    /// <inheritdoc/>
    public abstract bool TryGetItem(TKey key, bool markAsHit, out TItem item);
  }
}

