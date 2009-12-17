// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that persistent property must be included into full-text index.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class FullTextAttribute : StorageAttribute
  {
  }
}