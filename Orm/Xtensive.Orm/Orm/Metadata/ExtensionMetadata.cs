// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Non-persistent user-defined metadata.
  /// <seealso cref="Extension"/>.
  /// </summary>
  public class ExtensionMetadata
  {
    /// <summary>
    /// Gets the name of the metadata.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets metadata value (<see cref="Extension.Text"/>).
    /// </summary>
    public string Value { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }

    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The name of the metadata.</param>
    /// <param name="value">The value of the metadata.</param>
    public ExtensionMetadata(string name, string value)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");
      Name = name;
      Value = value;
    }
  }
}