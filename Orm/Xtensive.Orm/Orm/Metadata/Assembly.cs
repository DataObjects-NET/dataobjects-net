// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;


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
    [Field(Length = 500)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    [Field(Length = 64)]
    public string Version { get; set; }


    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format(Strings.MetadataAssemblyFormat, Name, Version);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The assembly name.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Assembly(string name) 
      : base(name)
    {
    }
  }
}