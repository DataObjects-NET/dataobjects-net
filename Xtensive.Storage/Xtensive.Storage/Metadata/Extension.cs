// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Persistent descriptor of an assembly with registered persistent types.
  /// Used for schema upgrade purposes.
  /// </summary>
  [SystemType(2)]
  [HierarchyRoot("Name")]
  [Entity(MappingName = "Metadata.Extension")]
  public class Extension : MetadataBase
  {
    /// <summary>
    /// Gets or sets the name of the extension.
    /// </summary>
    [Field(Length = 1024)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the extension data.
    /// </summary>
    [Field]
    public string Data { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The assembly name.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Extension(string name) 
      : base(Tuple.Create(name))
    {
    }
  }
}