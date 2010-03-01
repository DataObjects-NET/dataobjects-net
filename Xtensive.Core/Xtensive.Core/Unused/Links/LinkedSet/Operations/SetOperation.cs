// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  internal abstract class SetOperation<TOwner, TItem, TArg> : Operation<TArg>
    where TItem : ILinkOwner
    where TOwner : ILinkOwner
  {
    protected internal Func<TOwner, LinkedSet<TOwner, TItem>> linkedSetGetter;

    internal Func<TOwner, LinkedSet<TOwner, TItem>> LinkedSetGetter
    {
      get { return linkedSetGetter; }
      set
      {
        if (IsLocked)
          throw new InstanceIsLockedException();
        if (linkedSetGetter!=null)
          throw Exceptions.AlreadyInitialized("LinkedSetGetter");
        linkedSetGetter = value;
      }
    }

    protected SetOperation()
    {
    }
  }
}