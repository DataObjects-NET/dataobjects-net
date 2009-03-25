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
  /// Persistent descriptor of an assembly with registered persistent types.
  /// Used for schema upgrade purposes.
  /// </summary>
  [SystemType(TypeId = 2)]
  [HierarchyRoot("Id", InheritanceSchema = InheritanceSchema.ClassTable)]
  public class Assembly : Entity
  {
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [Field]
    public int Id { get; internal set; }

    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    /// <value>The name of the assembly.</value>
    [Field(Length = 500)]
    public string AssemblyName { get; set; }

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    [Field(Length = 50)]
    public string Version { get; internal set; }
  }
}