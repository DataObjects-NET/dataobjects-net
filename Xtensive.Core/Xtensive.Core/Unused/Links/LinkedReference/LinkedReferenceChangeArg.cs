// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.06

namespace Xtensive.Core.Links.LinkedReference
{
  public struct LinkedReferenceChangeArg<TOwner, TReference>
    where TOwner : class, ILinkOwner
    where TReference : class, ILinkOwner
  {
    public LinkedReference<TOwner, TReference> LinkedReference;
    public TReference NewValue;
    public TReference OldValue;

    public LinkedReferenceChangeArg(TReference oldValue, TReference newValue,
      LinkedReference<TOwner, TReference> linkedReference)
    {
      OldValue = oldValue;
      NewValue = newValue;
      LinkedReference = linkedReference;
    }
  }
}