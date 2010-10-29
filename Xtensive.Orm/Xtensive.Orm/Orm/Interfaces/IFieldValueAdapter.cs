// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.04

using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// An object exposing (i.e. providing access to) field value in custom fashion.
  /// </summary>
  public interface IFieldValueAdapter
  {
    /// <summary>
    /// Gets the owner of the value.
    /// </summary>
    Persistent Owner { get; }

    /// <summary>
    /// Gets the field this adapter handles.
    /// </summary>
    FieldInfo Field { get; }
  }
}