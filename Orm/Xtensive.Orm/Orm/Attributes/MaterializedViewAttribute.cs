// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.28

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// <b>Not yet supported.</b>
  /// Indicates that materialized view should be created for
  /// the interface type it is applied on.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
  public sealed class MaterializedViewAttribute : StorageAttribute
  {
  }
}