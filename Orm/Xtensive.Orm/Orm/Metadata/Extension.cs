// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Persistent value of any kind indentified by its <see cref="Name"/>.
  /// </summary>
  [Serializable]
  [SystemType(3)]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  [TableMapping("Metadata.Extension")]
  public sealed class Extension : Entity
  {
    /// <summary>
    /// Gets or sets the name of the extension.
    /// </summary>
    [Key]
    [Field(Length = 1024)]
    public string Name
    {
      get { return GetFieldValue<string>("Name"); }
    }

    /// <summary>
    /// Gets or sets the text data.
    /// </summary>
    [Field(Length = int.MaxValue)]
    public string Text
    {
      get { return GetFieldValue<string>("Text"); }
      set { SetFieldValue("Text", value); }
    }

    /// <summary>
    /// Gets or sets the binary data.
    /// </summary>
    [Field(Length = int.MaxValue)]
    public byte[] Data
    {
      get { return GetFieldValue<byte[]>("Data"); }
      set { SetFieldValue("Data", value); }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }

    private static Extension CreateObject(Session session, EntityState state)
    {
      return new Extension(session, state);
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">A value for <see cref="Name"/>.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Extension(string name) 
      : base(name)
    {
    }

    private Extension(Session session, EntityState state)
      : base(session, state)
    {
    }
  }
}