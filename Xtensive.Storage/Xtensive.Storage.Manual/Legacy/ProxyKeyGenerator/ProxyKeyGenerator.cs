// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.28

using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Manual.Legacy.ProxyKeyGenerator
{
  public sealed class ProxyKeyGenerator<TKeyConsumer, TKeySource> : KeyGenerator
    where TKeyConsumer : Entity
    where TKeySource : Entity
  {
    public override Tuple Next()
    {
      var key = Key.Create<TKeySource>(Handlers.Domain);
      // In any version before v4.1 final:
      // var key = Key.Create<TKeySource>();
      return key.Value;
    }
    
    public ProxyKeyGenerator(KeyProviderInfo keyProviderInfo)
      : base(keyProviderInfo)
    {}
  }
}