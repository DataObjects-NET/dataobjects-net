// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;

namespace Xtensive.Storage.Attributes
{
  [Serializable]
  public abstract class MappingAttribute : StorageAttribute
  {
    private string mappingName = string.Empty;

    /// <summary>
    /// Gets or sets the base part of the field's related column name 
    /// or the base part of the class' related table name.
    /// </summary>
    /// <remarks>
    /// You can use the following characters in <see cref="MappingName"/>s: [_A-Za-z0-9-.]. 
    /// <see cref="MappingName"/> can't be an empty string or <see langword="null"/>.
    /// </remarks>
    public string MappingName
    {
      get { return mappingName; }
      set { mappingName = value; }
    }
  }
}