// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.02

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  internal struct SetOperationArg<TOwner, TItem>
    where TOwner : ILinkOwner where TItem : ILinkOwner
  {
    public LinkedSet<TOwner, TItem> LinkedSet;

    public SetOperationArg(LinkedSet<TOwner, TItem> linkedSet)
    {
      LinkedSet = linkedSet;
    }
  }
}