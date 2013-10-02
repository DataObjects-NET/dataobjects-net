// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Collections
{
  /// <summary>
  /// A sequence of <typeparamref name="TKey"/>-<typeparamref name="TFlag"/> pairs.
  /// </summary>
  /// <remarks>
  /// Item count should be less than 32.
  /// <see cref="Biconverter{TFrom,TTo}"/> is used to convert flag keys from type <typeparamref name="TFlag"/> to <see cref="bool"/>.
  /// </remarks>
  /// <typeparam name="TKey">Type of the key.</typeparam>
  /// <typeparam name="TFlag">Type of the flag.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class FlagCollection<TKey, TFlag>: LockableBase,
    IList<KeyValuePair<TKey, TFlag>>,
    IDictionary<TKey, TFlag>,
    IEquatable<FlagCollection<TKey, TFlag>>,
    ISerializable
  {
    private const int MaxItemCount = 32;
    private readonly Biconverter<TFlag, bool> converter;
    private readonly List<TKey> keys;
    private readonly ReadOnlyList<TKey> readOnlyKeys;
    private BitVector32 flags;

    /// <summary>
    /// Gets <see cref="Biconverter{TFrom,TTo}"/> instance
    /// used to convert flag value to <see cref="bool"/> and vice versa.
    /// </summary>
    public Biconverter<TFlag, bool> Converter
    {
      [DebuggerStepThrough]
      get { return converter; }
    }

    /// <summary>
    /// Gets an <see cref="Collection{T}"/> containing the flags.
    /// </summary>
    public ICollection<TFlag> Flags
    {
      [DebuggerStepThrough]
      get { return Values; }
    }

    #region IDictionary<TKey,TFlag> Members

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      return keys.Contains(key);
    }

    /// <inheritdoc/>
    public void Add(TKey key, TFlag flag)
    {
      this.EnsureNotLocked();
      if (keys.Contains(key))
        throw new ArgumentException("key", Strings.ExCollectionAlreadyContainsSpecifiedItem);
      if (keys.Count >= MaxItemCount)
        throw new InvalidOperationException(string.Format(Strings.ExMaxItemCountIsN, MaxItemCount));
      keys.Add(key);
      flags[1 << (keys.Count - 1)] = converter.ConvertForward(flag);
    }

    /// <inheritdoc/>
    public virtual void Add(TKey key)
    {
      Add(key, default(TFlag));
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
      ArgumentValidator.EnsureArgumentIsNotDefault(key, "key");
      this.EnsureNotLocked();
      int index = keys.IndexOf(key);
      if (index < 0)
        return false;
      keys.RemoveAt(index);
      int data = flags.Data;
      int remainder = data & (0xFFFF << index);
      data ^= remainder;
      data |= (remainder >> 1) & (0xFFFF << index);
      flags = new BitVector32(data);
      return true;
    }

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TFlag value)
    {
      value = converter.ConvertBackward(false);
      int index = keys.IndexOf(key);
      if (index < 0)
        return false;
      value = converter.ConvertBackward(flags[1 << index]);
      return true;
    }

    /// <inheritdoc/>
    public TFlag this[TKey key]
    {
      get
      {
        ArgumentValidator.EnsureArgumentIsNotDefault(key, "key");
        TFlag value;
        bool result = TryGetValue(key, out value);
        if (!result)
          throw new KeyNotFoundException();
        return value;
      }
      set
      {
        ArgumentValidator.EnsureArgumentIsNotDefault(key, "key");
        this.EnsureNotLocked();
        int index = keys.IndexOf(key);
        if (index < 0)
          Add(key, value);
        else
          flags[1 << index] = converter.ConvertForward(value);
      }
    }

    /// <summary>
    /// Gets a list of keys.
    /// </summary>
    /// <returns>A list of keys.</returns>
    public ReadOnlyList<TKey> Keys
    {
      get { return readOnlyKeys; }
    }

    /// <inheritdoc/>
    ICollection<TKey> IDictionary<TKey, TFlag>.Keys
    {
      get { return readOnlyKeys; }
    }

    /// <summary>
    /// Gets an array of values.
    /// </summary>
    /// <returns>An array of values.</returns>
    public TFlag[] Values {
      get {
        var array = new TFlag[keys.Count];
        for (int i = 0; i < keys.Count; i++)
          array[i] = converter.ConvertBackward(flags[1 << i]);
        return array;
      }
    }

    /// <inheritdoc/>
    ICollection<TFlag> IDictionary<TKey, TFlag>.Values {
      get {
        var list = new List<TFlag>(keys.Count);
        for (int i = 0; i < keys.Count; i++) {
          list.Add(converter.ConvertBackward(flags[1 << i]));
        }
        return list;
      }
    }

    #endregion

    #region IList<KeyValuePair<TKey,TFlag>> Members

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TFlag> item)
    {
      Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      this.EnsureNotLocked();
      keys.Clear();
      flags = new BitVector32(0);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TFlag> item)
    {
      return ContainsKey(item.Key);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TFlag>[] array, int arrayIndex)
    {
      this.Copy(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TFlag> item)
    {
      return Remove(item.Key);
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return keys.Count; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return IsLocked; }
    }

    /// <inheritdoc/>
    int IList<KeyValuePair<TKey, TFlag>>.IndexOf(KeyValuePair<TKey, TFlag> item)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    void IList<KeyValuePair<TKey, TFlag>>.Insert(int index, KeyValuePair<TKey, TFlag> item)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    void IList<KeyValuePair<TKey, TFlag>>.RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public KeyValuePair<TKey, TFlag> this[int index] {
      get {
        if (keys.Count <= index)
          throw new ArgumentOutOfRangeException("index");
        return new KeyValuePair<TKey, TFlag>(keys[index], converter.ConvertBackward(flags[1<<index]));
      }
      set {
        // TODO: implement?
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey, TFlag>> Members

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TKey, TFlag>>.Add(KeyValuePair<TKey, TFlag> key)
    {
      Add(key.Key, key.Value);
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TFlag>>.Contains(KeyValuePair<TKey, TFlag> item)
    {
      int index = keys.IndexOf(item.Key);
      if (index < 0)
        return false;
      return flags[1 << index] == converter.ConvertForward(item.Value);
    }

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TKey, TFlag>>.CopyTo(KeyValuePair<TKey, TFlag>[] array, int arrayIndex)
    {
      this.Copy(array, arrayIndex);
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TFlag>>.Remove(KeyValuePair<TKey, TFlag> item)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region GetEnumerator methods

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TFlag>> GetEnumerator()
    {
      for (int i = 0; i < keys.Count; i++)
        yield return new KeyValuePair<TKey, TFlag>(keys[i], converter.ConvertBackward(flags[1 << i]));
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<TKey, TFlag>>)this).GetEnumerator();
    }

    #endregion

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(FlagCollection<TKey, TFlag> other)
    {
      if (other == null)
        return false;
      if (Count != other.Count)
        return false;
      for (int i = 0; i < Count; i++)
        if (!this[i].Equals(other[i]))
          return false;
      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as FlagCollection<TKey, TFlag>);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = keys.GetHashCode();
      result = 29*result + flags.GetHashCode();
      return result;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="converter"><see cref="Converter"/> property value.</param>
    public FlagCollection(Biconverter<TFlag, bool> converter)
      : this()
    {
      this.converter = converter;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="converter"><see cref="Converter"/> property value.</param>
    /// <param name="enumerable">Initial content of collection.</param>
    public FlagCollection(Biconverter<TFlag, bool> converter, IEnumerable<KeyValuePair<TKey, TFlag>> enumerable)
      : this()
    {
      this.converter = converter;
      foreach (KeyValuePair<TKey, TFlag> pair in enumerable)
        Add(pair.Key, pair.Value);
    }

    private FlagCollection()
    {
      keys = new List<TKey>();
      readOnlyKeys = new ReadOnlyList<TKey>(keys);
    }

    #region ISerializable members

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected FlagCollection(SerializationInfo info, StreamingContext context)
      : base(info.GetBoolean("IsLocked"))
    {
      converter = (Biconverter<TFlag, bool>)
        info.GetValue("AdvancedConverter", typeof(Biconverter<TFlag, bool>));
      keys = (List<TKey>)info.GetValue("Keys", typeof(List<TKey>));
      readOnlyKeys = new ReadOnlyList<TKey>(keys);
      flags = new BitVector32(info.GetInt32("Flags"));
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("IsLocked", IsLocked);
      info.AddValue("AdvancedConverter", converter);
      info.AddValue("Keys", keys);
      info.AddValue("Flags", flags.Data);
    }

    #endregion
  }
}