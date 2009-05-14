// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Persistent descriptor of registered type.
  /// </summary>
  [SystemType(1)]
  [HierarchyRoot("Id")]
  [Entity(MappingName = "Metadata.Type")]
  [Index("Name", IsUnique = true)]
  public class Type : Entity
  {
    /// <summary>
    /// Gets or sets the type identifier.
    /// </summary>
    [Field]
    public int Id { get; private set; }

    /// <summary>
    /// Gets or sets the full type name.
    /// </summary>
    [Field(Length = 2048)]
    public string Name { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataTypeFormat, Name, Id);
    }

    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="id">The type identifier.</param>
    /// <param name="name">The name of the type.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Type(int id, string name) 
      : base(Tuple.Create(id))
    {
      Name = name;
    }
  }
}