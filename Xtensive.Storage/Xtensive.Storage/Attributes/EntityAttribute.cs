// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.01

using System;

namespace Xtensive.Storage.Attributes
{
  /// <summary>
  /// Defines mapping name for persistent class (i.e. name of the table this class is mapped to).
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class EntityAttribute: MappingAttribute
  {
  }
}