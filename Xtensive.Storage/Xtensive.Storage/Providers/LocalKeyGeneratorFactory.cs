// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.18

using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  internal sealed class LocalKeyGeneratorFactory : KeyGeneratorFactory
  {
    /// <inheritdoc/>
    public override bool IsSchemaBoundGenerator(KeyProviderInfo keyProviderInfo)
    {
      return false;
    }

    /// <inheritdoc/>
    protected override KeyGenerator CreateGenerator<TFieldType>(KeyProviderInfo keyProviderInfo)
    {
      return new LocalKeyGenerator<TFieldType>(keyProviderInfo);
    }
  }
}