// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal sealed class KeyGeneratorRegistry : LockableBase
  {
    private readonly Dictionary<KeyInfo, KeyGenerator> generators
      = new Dictionary<KeyInfo, KeyGenerator>();

    private readonly Dictionary<KeyInfo, TemporaryKeyGenerator> temporaryGenerators
      = new Dictionary<KeyInfo, TemporaryKeyGenerator>();

    // Compatibility indexer
    public KeyGenerator this[KeyInfo key] { get { return Get(key); } }

    public KeyGenerator Get(KeyInfo key)
    {
      KeyGenerator result;
      generators.TryGetValue(key, out result);
      return result;
    }

    public KeyGenerator Get(KeyInfo key, bool isTemporary)
    {
      return isTemporary ? GetTemporary(key) : Get(key);
    }

    public TemporaryKeyGenerator GetTemporary(KeyInfo key)
    {
      TemporaryKeyGenerator result;
      temporaryGenerators.TryGetValue(key, out result);
      return result;
    }

    public void Register(KeyInfo key, KeyGenerator generator)
    {
      this.EnsureNotLocked();
      generators.Add(key, generator);
    }

    public void RegisterTemporary(KeyInfo key, TemporaryKeyGenerator temporaryGenerator)
    {
      this.EnsureNotLocked();
      temporaryGenerators.Add(key, temporaryGenerator);
    }
  }
}