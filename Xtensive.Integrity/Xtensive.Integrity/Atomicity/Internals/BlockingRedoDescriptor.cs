// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using Xtensive.Integrity.Aspects;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Atomicity.Internals
{
  [Serializable]
  internal sealed class BlockingRedoDescriptor: RedoDescriptor
  {
    public override void Invoke()
    {
      throw new InvalidOperationException(Strings.ExCantInvokeBlockingDescriptor);
    }
  }
}