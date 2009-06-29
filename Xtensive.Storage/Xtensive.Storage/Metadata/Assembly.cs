// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Persistent descriptor of an assembly with registered persistent types.
  /// Used for schema upgrade purposes.
  /// </summary>
  [SystemType(2)]
  [HierarchyRoot]
  [KeyGenerator(null)]
  [Mapping("Metadata.Assembly")]
  public class Assembly : MetadataBase
  {
    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    [Key]
    [Field(Length = 1024)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    [Field(Length = 64)]
    public string Version { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataAssemblyFormat, Name, Version);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The assembly name.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Assembly(string name) 
      : base(name)
    {
    }
  }
}