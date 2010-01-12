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
    [Field(Length = 1000)]
    public string Name { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataTypeFormat, Name, Id);
    }

    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
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