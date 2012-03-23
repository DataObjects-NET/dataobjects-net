// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System.Collections.Generic;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataSet
  {
    public readonly List<AssemblyMetadata> Assemblies = new List<AssemblyMetadata>();
    public readonly List<ExtensionMetadata> Extensions = new List<ExtensionMetadata>();
    public readonly List<TypeMetadata> Types = new List<TypeMetadata>();

    public void UnionWith(MetadataSet other)
    {
      Assemblies.AddRange(other.Assemblies);
      Extensions.AddRange(other.Extensions);
      Types.AddRange(other.Types);
    }
  }
}