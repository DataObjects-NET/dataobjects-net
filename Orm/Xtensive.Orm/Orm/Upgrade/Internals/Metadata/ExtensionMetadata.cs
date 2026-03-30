// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class ExtensionMetadata
  {
    public string Name { get; private set; }

    public string Value { get; private set; }

    public override string ToString()
    {
      return Name;
    }

    // Constructors

    public ExtensionMetadata(string name, string value)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Value = value;
    }
  }
}