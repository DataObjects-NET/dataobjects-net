// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Marks persistent property as type discriminator.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class TypeDiscriminatorAttribute : StorageAttribute
  {
  }
}