// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.07.10

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Field mapping attribute.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class FieldMappingAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the base part of the field's related column name.
    /// </summary>
    /// <remarks>
    /// You can use the following characters in <see cref="Name"/>s: [_A-Za-z0-9-.]. 
    /// <see cref="Name"/> can't be an empty string or <see langword="null"/>.
    /// </remarks>
    public string Name { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">Field mapping name.</param>
    public FieldMappingAttribute(string name)
    {
      Name = name;
    }
  }
}