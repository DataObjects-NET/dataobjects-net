// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;

namespace Xtensive.Storage.Attributes
{
  /// <summary>
  /// Marks persistent type as a system type.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  internal class SystemTypeAttribute : Attribute
  {
    /// <summary>
    /// Type identifier.
    /// </summary>
    public new int TypeId { get; set; }
  }
}