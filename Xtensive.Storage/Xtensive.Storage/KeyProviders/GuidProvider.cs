// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.KeyProviders
{
  [KeyProvider(typeof (Guid))]
  public class GuidProvider : KeyProviderBase
  {
    /// <inheritdoc/>
    public override void GetNext(Tuple keyTuple)
    {
      keyTuple.SetValue(0, Guid.NewGuid());
    }
  }
}