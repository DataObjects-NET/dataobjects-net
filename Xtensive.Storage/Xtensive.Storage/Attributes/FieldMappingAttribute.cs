// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.07.10

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Field mapping attribute.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
  public class FieldMappingAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the base part of the field's related column name.
    /// </summary>
    /// <remarks>
    /// You can use the following characters in <see cref="Name"/>s: [_A-Za-z0-9-.]. 
    /// <see cref="Name"/> can't be an empty string or <see langword="null"/>.
    /// </remarks>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the base part of the field's related column name.
    /// </summary>
    /// <remarks>
    /// You can use the following characters in <see cref="FieldName"/>s: [_A-Za-z0-9-.]. 
    /// <see cref="FieldName"/> can't be an empty string or <see langword="null"/>.
    /// </remarks>
    public string FieldName { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingName">Field mapping name.</param>
    public FieldMappingAttribute(string mappingName)
    {
      Name = mappingName;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingName">Field mapping name.</param>
    /// <param name="fieldName">Field name.</param>
    public FieldMappingAttribute(string mappingName, string fieldName)
    {
      Name = mappingName;
      FieldName = fieldName;
    }
  }
}