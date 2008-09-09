// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal abstract class WrappingEntitySet<T1> : EntitySet<T1>
    where T1 : Entity
  {
    public WrappingEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}