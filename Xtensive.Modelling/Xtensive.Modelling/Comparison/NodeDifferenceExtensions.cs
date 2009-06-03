// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.31

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Comparison
{
  public static class NodeDifferenceExtensions
  {
    public static bool IsRemoved(this NodeDifference difference)
    {
      return (difference.MovementInfo & MovementInfo.Removed)!=0;
    }

    public static bool IsCreated(this NodeDifference difference)
    {
      return (difference.MovementInfo & MovementInfo.Created)!=0;
    }

    public static bool IsChanged(this NodeDifference difference)
    {
      return (difference.MovementInfo & MovementInfo.Changed)!=0;
    }

    public static bool IsNameChanged(this NodeDifference difference)
    {
      return (difference.MovementInfo & MovementInfo.NameChanged)!=0;
    }
  }
}