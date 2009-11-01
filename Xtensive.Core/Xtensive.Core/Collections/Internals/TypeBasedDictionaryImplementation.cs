// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.20

namespace Xtensive.Core.Collections
{
  internal abstract class TypeBasedDictionaryImplementation
  {
    public abstract TItem GetValue<TKey, TItem>();
    public abstract void  SetValue<TKey, TItem>(TItem item);
  }

  internal class TypeBasedDictionaryImplementation<TVariator>: TypeBasedDictionaryImplementation
  {
    public override void SetValue<TKey, TItem>(TItem item)
    {
      TypeBasedDictionaryItem<TKey, TItem, TVariator>.Value = item;
    }

    public override TItem GetValue<TKey, TItem>()
    {
      return TypeBasedDictionaryItem<TKey, TItem, TVariator>.Value;
    }
  }
}