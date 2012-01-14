// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System;

namespace Xtensive.Orm.Building.FixupActions
{
  [Serializable]
  internal abstract class FixupAction
  {
    public abstract void Run();
  }
}