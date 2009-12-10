// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Diagnostics;
using System.Reflection;

namespace Xtensive.Core.ObjectMapping
{
  [Serializable]
  [DebuggerDisplay("Source = {Source}, Type = {Type}")]
  public struct ModificationDescriptor
  {
    public readonly object Source;

    public readonly PropertyInfo Property;

    public readonly ModificationType Type;

    public readonly object NewValue;


    // Constructors

    public ModificationDescriptor(object source, ModificationType type, PropertyInfo property, object data)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      Source = source;
      Type = type;
      Property = property;
      NewValue = data;
    }
  }
}