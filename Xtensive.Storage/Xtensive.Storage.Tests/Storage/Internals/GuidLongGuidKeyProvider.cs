// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.28

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Internals
{
  [KeyProvider(typeof (Guid), typeof (long), typeof (Guid))]
  public class GuidLongGuidKeyProvider : KeyProviderBase
  {
    private long currentLongValue = 1;

    public override void GetNext(Tuple keyTuple)
    {
      keyTuple.SetValue(0, Guid.NewGuid());
      keyTuple.SetValue(1, currentLongValue++);
      keyTuple.SetValue(2, Guid.NewGuid());
    }
  }
}