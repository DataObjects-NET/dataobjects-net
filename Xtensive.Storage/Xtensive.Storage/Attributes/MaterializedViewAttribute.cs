// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.28

using System;

namespace Xtensive.Storage.Attributes
{
  // TODO: Rename
  [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
  public class MaterializedViewAttribute : MappingAttribute
  {}
}