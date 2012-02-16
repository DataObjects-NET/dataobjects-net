// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Non-persistent type metadata.
  /// </summary>
  public class TypeMetadata
  {
    /// <summary>
    /// Gets the type identifier.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the full type name.
    /// </summary>
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
    public TypeMetadata(int id, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");

      Id = id;
      Name = name;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="id">The type identifier.</param>
    /// <param name="type">The type.</param>
    public TypeMetadata(int id, System.Type type)
    {
      Id = id;
      Name = type.GetFullName();
    }
  }
}