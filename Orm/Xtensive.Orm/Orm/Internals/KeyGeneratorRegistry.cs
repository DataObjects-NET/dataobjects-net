// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal sealed class KeyGeneratorRegistry : LockableBase
  {
    private readonly Dictionary<KeyInfo, IKeyGenerator> generators
      = new Dictionary<KeyInfo, IKeyGenerator>();

    private readonly Dictionary<KeyInfo, ITemporaryKeyGenerator> temporaryGenerators
      = new Dictionary<KeyInfo, ITemporaryKeyGenerator>();

    // Compatibility indexer
    public IKeyGenerator this[KeyInfo key] { get { return Get(key); } }

    public IKeyGenerator Get(KeyInfo key)
    {
      IKeyGenerator result;
      generators.TryGetValue(key, out result);
      return result;
    }

    public IKeyGenerator Get(KeyInfo key, bool isTemporary)
    {
      return isTemporary ? GetTemporary(key) : Get(key);
    }

    public ITemporaryKeyGenerator GetTemporary(KeyInfo key)
    {
      ITemporaryKeyGenerator result;
      temporaryGenerators.TryGetValue(key, out result);
      return result;
    }

    public void Register(KeyInfo key, IKeyGenerator generator)
    {
      this.EnsureNotLocked();
      generators.Add(key, generator);
    }

    public void RegisterTemporary(KeyInfo key, ITemporaryKeyGenerator temporaryGenerator)
    {
      this.EnsureNotLocked();
      temporaryGenerators.Add(key, temporaryGenerator);
    }
  }
}