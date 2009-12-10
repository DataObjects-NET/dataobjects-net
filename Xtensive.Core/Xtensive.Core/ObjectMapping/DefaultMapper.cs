// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;

namespace Xtensive.Core.ObjectMapping
{
  public class DefaultMapper : MapperBase
  {
    public event Action<ModificationDescriptor> ObjectModified;

    protected override void OnObjectModified(ModificationDescriptor descriptor)
    {
      ObjectModified.Invoke(descriptor);
    }
  }
}