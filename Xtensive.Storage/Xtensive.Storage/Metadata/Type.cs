// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Persistent descriptor of registered type.
  /// </summary>
  [SystemType(TypeId = 1)]
  [HierarchyRoot("Id", InheritanceSchema = InheritanceSchema.ClassTable)]
  public class Type : Entity
  {
    /// <summary>
    /// Gets or sets the type id.
    /// </summary>
    [Field]
    public int Id { get; internal set; }

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    [Field(Length = 1000)]
    public string FullName { get; internal set; }
  }
}