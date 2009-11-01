// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.06

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Internals
{
  [KeyProvider(typeof (string))]
  public class StringProvider : KeyProviderBase
  {
    public override void GetNext(Tuple keyTuple)
    {
      keyTuple.SetValue(0, Guid.NewGuid().ToString());
    }
  }
}