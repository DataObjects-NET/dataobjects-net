// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System;

namespace Xtensive.Storage.Building.FixupActions
{
  [Serializable]
  internal abstract class FixupAction
  {
    public abstract void Run();
  }
}