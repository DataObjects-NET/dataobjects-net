// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.04

using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public interface IFieldHandler
  {
    Persistent Owner { get; }

    FieldInfo Field { get; }
  }
}