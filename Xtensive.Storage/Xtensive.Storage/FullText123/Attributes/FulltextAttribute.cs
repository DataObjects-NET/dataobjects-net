// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Fulltext.Attributes
{
  /// <summary>
  /// Marks persistent property as fulltext indexed.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class FullTextAttribute : StorageAttribute
  {
    
  }
}