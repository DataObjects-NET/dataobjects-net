// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;


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
  public sealed class Assembly : MetadataBase
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    [Key]
    [Field(Length = 1024)]
    public string Name
    {
      get { return GetFieldValue<string>("Name"); }
    }

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    [Field(Length = 64)]
    public string Version
    {
      get { return GetFieldValue<string>("Version"); }
      set { SetFieldValue("Version", value); }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataAssemblyFormat, Name, Version);
    }

    private static Assembly CreateObject(Session session, EntityState state)
    {
      return new Assembly(session, state);
    }


    // Constructors

    private Assembly(Session session, EntityState state)
      : base(session, state)
    {
    }
  }
}