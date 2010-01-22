// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

namespace Xtensive.Storage.ObjectMapping
{
  /// <summary>
  /// Contract for a POCO object whose version should be validated.
  /// </summary>
  public interface IHasVersion
  {
    byte[] Version { get; set; }
  }
}