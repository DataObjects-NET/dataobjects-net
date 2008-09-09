// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.08

using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Providers.VistaDb
{
  public sealed class NameBuilder : Providers.NameBuilder
  {
    /// <inheritdoc/>
    protected override void Initialize(NamingConvention namingConvention)
    {
      NamingConvention clone = (NamingConvention)namingConvention.Clone();
      clone.NamingRules |= NamingRules.UnderscoreDots | NamingRules.UnderscoreHyphens;
      base.Initialize(clone);
    }
  }
}