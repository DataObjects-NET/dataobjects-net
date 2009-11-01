// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.30

using System;
using Xtensive.Integrity.Atomicity;

namespace Xtensive.Integrity.Aspects.Internals
{
  [Serializable]
  internal class AtomicAttributeTag
  {
    public AtomicityScope atomicityScope;
    public RedoScope RedoScope;
    public UndoScope UndoScope;
  }
}