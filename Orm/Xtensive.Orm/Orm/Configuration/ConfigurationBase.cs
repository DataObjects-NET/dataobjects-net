// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using Xtensive.Core;


namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Base class for configuration.
  /// </summary>
  [Serializable]
  public abstract class ConfigurationBase: LockableBase, ICloneable
  {
    /// <summary>
    /// Validates the configuration.
    /// Should always be invoked by <see cref="ILockable.Lock(bool)"/> method 
    /// before actually locking the configuration.
    /// </summary>
    public virtual void Validate()
    {
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      Validate();
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public virtual object Clone()
    {
      var clone = CreateClone();
      clone.CopyFrom(this);
      return clone;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// Used by <see cref="Clone"/> method implementation.
    /// </summary>
    /// <returns>New instance of this class.</returns>
    protected abstract ConfigurationBase CreateClone();

    /// <summary>
    /// Copies the properties from the <paramref name="source"/>
    /// configuration to this one.
    /// Used by <see cref="Clone"/> method implementation.
    /// </summary>
    /// <param name="source">The configuration to copy properties from.</param>
    protected virtual void CopyFrom(ConfigurationBase source)
    {
      // Does nothing in this class.
    }
  }
}