// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using Xtensive.Core;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class AssemblyMetadata
  {
    public string Name { get; private set; }

    public string Version { get; private set; }

    public override string ToString()
    {
      return string.Format(Strings.MetadataAssemblyFormat, Name, Version);
    }

    // Constructors

    public AssemblyMetadata(string name, string version)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Name = name;
      Version = version;
    }
  }
}