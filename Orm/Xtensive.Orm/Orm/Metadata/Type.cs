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
    public int Id
    {
      get { return GetFieldValue<int>("Id"); }
    }

    /// <summary>
    /// Gets or sets the full type name.
    /// </summary>
    [Field(Length = 1000)]
    public string Name
    {
      get { return GetFieldValue<string>("Name"); }
      set { SetFieldValue("Name", value); }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataTypeFormat, Name, Id);
    }

    private static Type CreateObject(Session session, EntityState state)
    {
      return new Type(session, state);
    }

    // Constructors

    private Type(Session session, EntityState state)
      : base(session, state)
    {
    }
  }
}