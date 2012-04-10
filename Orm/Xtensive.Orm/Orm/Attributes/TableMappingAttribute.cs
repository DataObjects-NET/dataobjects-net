// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Table mapping attribute.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, 
    AllowMultiple = false, Inherited = false)]
  public sealed class TableMappingAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the base part of the field's related column name 
    /// or the base part of the class' related table name.
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
    /// <param name="name"><see cref="Name"/> property value.</param>
    public TableMappingAttribute(string name)
    {
      Name = name;
    }
  }
}