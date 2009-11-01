// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.26

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.KeyProviders
{
  [KeyProvider(typeof(int))]
  [Serializable]
  public class Int32Provider : KeyProviderBase
  {
    private int current = 1;

    /// <inheritdoc/>
    public override void GetNext(Tuple keyTuple)
    {
      keyTuple.SetValue(0, current++);
    }
  }
}
