// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  internal sealed class SkipOwnerCheckScope : SimpleScope<SkipOwnerCheckScope.Variator>
  {
    internal abstract class Variator{}
    
    public static bool IsActive
    {
      get { return Current != null; }
    }
  }
}