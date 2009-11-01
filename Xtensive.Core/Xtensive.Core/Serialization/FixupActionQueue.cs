// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  public class FixupActionQueue : Queue<FixupAction>
  {
    public void Enqueue(IReference reference, Action<IReference> action)
    {
      Enqueue(new FixupAction(reference, action));
    }
  }
}