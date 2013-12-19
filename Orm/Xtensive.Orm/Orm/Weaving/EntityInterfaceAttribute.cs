// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.26

using System;
using JetBrains.Annotations;

namespace Xtensive.Orm.Weaving
{
  /// <summary>
  /// Identifies persistent interface.
  /// You should not use this attribute directly.
  /// It is automatically applied to your types when needed.
  /// </summary>
  [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
  [UsedImplicitly]
  public sealed class EntityInterfaceAttribute : Attribute
  {
  }
}