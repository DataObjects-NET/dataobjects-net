// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.14

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Describes measured item.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  [Serializable]
  public struct MeasuredItem<TKey, TValue>
  {
    /// <summary>
    /// Indicates whether key is new.
    /// </summary>
    public readonly bool KeyIsNew;

    /// <summary>
    /// The key.
    /// </summary>
    public readonly TKey Key;

    /// <summary>
    /// The value.
    /// </summary>
    public readonly TValue Value;

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is MeasuredItem<TKey, TValue>) {
        MeasuredItem<TKey, TValue> item = (MeasuredItem<TKey, TValue>)obj;
        return KeyIsNew==item.KeyIsNew && Equals(Key, item.Key) && Equals(Value, item.Value);
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int keyHash = Key==null ? 0 : Key.GetHashCode();
      int valueHash = Value==null ? 0 : Value.GetHashCode();
      return keyHash ^ valueHash ^ (KeyIsNew ? 1 : 0);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="keyIsNew">Indicates whether key is new.</param>
    public MeasuredItem(TKey key, TValue value, bool keyIsNew)
    {
      KeyIsNew = true;
      Key = key;
      Value = value;
      KeyIsNew = keyIsNew;
    }
  }
}