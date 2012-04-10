// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Abstract base class for any upgrade hint.
  /// </summary>
  [Serializable]
  public abstract class UpgradeHint :
    IEquatable<UpgradeHint>
  {
    
    public virtual bool Equals(UpgradeHint other)
    {
      return ReferenceEquals(this, other);
    }

    
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (UpgradeHint))
        return false;
      return Equals((UpgradeHint) obj);
    }

    
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }


    // Constructors

    internal UpgradeHint()
    {
    }
  }
}