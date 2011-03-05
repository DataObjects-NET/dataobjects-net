// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Persistent descriptor of an assembly with registered persistent types.
  /// Used for schema upgrade purposes.
  /// </summary>
  [Serializable]
  [SystemType(2)]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  [TableMapping("Metadata.Assembly")]
  public class Assembly : MetadataBase
  {
    /// <summary>
    /// Gets the name of the assembly.
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