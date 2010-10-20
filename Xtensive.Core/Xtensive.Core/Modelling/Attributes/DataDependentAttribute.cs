// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;

namespace Xtensive.Modelling.Attributes
{
  /// <summary>
  /// Data dependent node marker.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class DataDependentAttribute : Attribute
  {
  }
}