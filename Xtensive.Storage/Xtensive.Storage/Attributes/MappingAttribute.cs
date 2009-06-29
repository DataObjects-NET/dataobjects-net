// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for all mapping attributes.
  /// </summary>
  [Serializable]
  public sealed class MappingAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets or sets the base part of the field's related column name 
    /// or the base part of the class' related table name.
    /// </summary>
    /// <remarks>
    /// You can use the following characters in <see cref="Name"/>s: [_A-Za-z0-9-.]. 
    /// <see cref="Name"/> can't be an empty string or <see langword="null"/>.
    /// </remarks>
    public string Name { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingName"><see cref="Name"/> property value.</param>
    public MappingAttribute(string mappingName)
    {
      Name = mappingName;
    }
  }
}