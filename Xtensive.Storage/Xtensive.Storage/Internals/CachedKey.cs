// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.12

using Xtensive.Core;

namespace Xtensive.Storage.Internals
{
  internal class CachedKey : IIdentified<Key>,
    IHasSize
  {
    public Key Key { get; private set; }

    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    public Key Identifier
    {
      get { return Key; }
    }

    public long Size
    {
      get { return 1; }
    }

    public CachedKey(Key key)
    {
      Key = key;
    }
  }
}