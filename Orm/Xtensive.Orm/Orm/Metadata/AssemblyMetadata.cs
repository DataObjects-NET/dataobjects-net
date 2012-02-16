// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Non-persistent assembly metadata.
  /// <seealso cref="Assembly"/>.
  /// </summary>
  public sealed class AssemblyMetadata
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the assembly version.
    /// </summary>
    public string Version { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataAssemblyFormat, Name, Version);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Name of the assembly.</param>
    public AssemblyMetadata(string name)
      : this(name, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Name of the assembly.</param>
    /// <param name="version">Assembly version.</param>
    public AssemblyMetadata(string name, string version)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Name = name;
      Version = version;
    }
  }
}