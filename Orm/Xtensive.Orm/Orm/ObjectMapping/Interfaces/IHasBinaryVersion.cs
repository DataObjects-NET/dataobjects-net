// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

namespace Xtensive.Orm.ObjectMapping
{
  /// <summary>
  /// Contract for a POCO object whose binary version should be validated.
  /// </summary>
  public interface IHasBinaryVersion
  {
    /// <summary>
    /// Gets or sets the binary version of an object.
    /// </summary>
    byte[] Version { get; set; }
  }
}