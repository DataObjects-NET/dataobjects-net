// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.14

using Xtensive.Core.Aspects;

namespace Xtensive.Storage
{
  /// <summary>
  /// A marker interface for entity sets.
  /// </summary>
  [EntitySetAspect]
  [Initializable]
  public interface IEntitySet
  {
  }
}