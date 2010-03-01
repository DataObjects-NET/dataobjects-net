// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.14

using System;

namespace Xtensive.Indexing
{
  [Serializable]
  public struct MeasuredItem<TKey, TValue>
  {
    public readonly bool KeyIsNew;
    public readonly TKey Key;
    public readonly TValue Value;

    public override bool Equals(object obj)
    {
      if (obj is MeasuredItem<TKey, TValue>) {
        MeasuredItem<TKey, TValue> item = (MeasuredItem<TKey, TValue>)obj;
        return KeyIsNew==item.KeyIsNew && Equals(Key, item.Key) && Equals(Value, item.Value);
      }
      return false;
    }

    public override int GetHashCode()
    {
      int keyHash = Key==null ? 0 : Key.GetHashCode();
      int valueHash = Value==null ? 0 : Value.GetHashCode();
      return keyHash ^ valueHash ^ (KeyIsNew ? 1 : 0);
    }


    // Constructors

    public MeasuredItem(TKey key, TValue value, bool keyIsNew)
    {
      KeyIsNew = true;
      Key = key;
      Value = value;
      KeyIsNew = keyIsNew;
    }
  }
}