// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;


namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Persistent descriptor of registered type.
  /// </summary>
  [Serializable]
  [SystemType(1)]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  [TableMapping("Metadata.Type")]
  [Index("Name", Unique = true)]
  public class Type : MetadataBase
  {
    /// <summary>
    /// Gets or sets the type identifier.
    /// </summary>
    [Key]
    [Field]
    public int Id { get; private set; }

    /// <summary>
    /// Gets or sets the full type name.
    /// </summary>
    [Field(Length = 500)]
    public string Name { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataTypeFormat, Name, Id);
    }

    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="id">The type identifier.</param>
    /// <param name="name">The name of the type.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Type(int id, string name) 
      : base(id)
    {
      Name = name;
    }
  }
}