// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;


namespace Xtensive.Modelling.Attributes
{
  /// <summary>
  /// System property marker.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  [Serializable]
  internal sealed class SystemPropertyAttribute : Attribute
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    internal SystemPropertyAttribute()
    {
    }
  }
}