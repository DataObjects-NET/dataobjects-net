// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Attributes;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Persistent descriptor of registered type.
  /// </summary>
  [SystemType(TypeId = 1)]
  [HierarchyRoot("Id")]
  [Index("Name", IsUnique = true)]
  public class Type : Entity
  {
    /// <summary>
    /// Gets or sets the type identifier.
    /// </summary>
    [Field]
    public int Id { get; private set; }

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    [Field(Length = 1000)]
    public string Name { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="id">The type id.</param>
    public Type(int id) 
      : base(Tuple.Create(id))
    {
    }
  }
}