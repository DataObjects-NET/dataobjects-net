// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.05

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  public struct SetChangedArg<TOwner, TItem>
    where TOwner : ILinkOwner where TItem : ILinkOwner
  {
    public TItem Item;
    public LinkedSet<TOwner, TItem> LinkedSet;

    public SetChangedArg(LinkedSet<TOwner, TItem> linkedSet, TItem item)
    {
      LinkedSet = linkedSet;
      Item = item;
    }
  }
}