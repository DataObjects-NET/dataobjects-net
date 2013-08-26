// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.26

using System;

namespace Xtensive.Orm.Weaver
{
  /// <summary>
  /// Identifies persistent property.
  /// You should not use this attribute directly.
  /// It is automatically applied to your types when needed.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class PersistentPropertyAttribute : Attribute
  {
  }
}