// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.13

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Weaving
{
  /// <summary>
  /// Overrides field name for persistence purposes. Should not be applied in end-user code.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class OverrideFieldNameAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="OverrideFieldNameAttribute"/> class.
    /// </summary>
    public OverrideFieldNameAttribute(string name)
    {
      Name = name;
    }
  }
}