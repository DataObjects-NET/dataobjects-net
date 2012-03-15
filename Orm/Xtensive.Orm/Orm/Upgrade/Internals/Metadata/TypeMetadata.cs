// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class TypeMetadata
  {
    public int Id { get; private set; }

    public string Name { get; set; }

    public override string ToString()
    {
      return string.Format(Strings.MetadataTypeFormat, Name, Id);
    }

    // Constructors

    public TypeMetadata(int id, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");

      Id = id;
      Name = name;
    }

    public TypeMetadata(int id, System.Type type)
    {
      Id = id;
      Name = type.GetFullName();
    }
  }
}