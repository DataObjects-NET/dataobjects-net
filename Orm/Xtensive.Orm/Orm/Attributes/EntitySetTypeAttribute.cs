// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.22

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Identifies persistent <see cref="EntitySet{TItem}"/> type.
  /// You should not use this attribute directly.
  /// It is automatically applied to your types when needed.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public sealed class EntitySetTypeAttribute : StorageAttribute
  {
  }
}