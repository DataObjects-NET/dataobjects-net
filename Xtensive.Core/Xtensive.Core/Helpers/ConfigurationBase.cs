// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// Base class for <see cref="IConfiguration"/> implementors.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate"/></para>
  /// </remarks>
  [Serializable]
  public abstract class ConfigurationBase: LockableBase,
    IConfiguration
  {
    /// <inheritdoc/>
    public abstract void Validate();

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      Validate();
      base.Lock(recursive);
    }

    #region Clone method implementation

    /// <inheritdoc/>
    public virtual object Clone()
    {
      ConfigurationBase clone = CreateClone();
      clone.Clone(this);
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
    protected virtual void Clone(ConfigurationBase source)
    {
      // Does nothing in this class.
    }

    #endregion
  }
}