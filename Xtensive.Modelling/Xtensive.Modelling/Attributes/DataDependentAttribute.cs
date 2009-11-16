// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Attributes
{
  /// <summary>
  /// Data dependent node marker.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class DataDependentAttribute : Attribute
  {

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DataDependentAttribute()
    {
    }
  }
}