// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using System.Configuration;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Configuration
{
  /// <summary>
  /// Base class for <see cref="IConfiguration"/> implementors with 
  /// support of reading from application configuration file.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate"/></para>
  /// </remarks>
  [Serializable]
  public abstract class ConfigurationSectionBase : ConfigurationSection,
    IConfiguration
  {
    #region Lockable

    private bool isLocked;

    /// <inheritdoc/>
    public bool IsLocked
    {
      get { return isLocked; }
    }

    /// <inheritdoc/>
    public void Lock()
    {
      Lock(true);
    }

    /// <inheritdoc/>
    public virtual void Lock(bool recursive)
    {
      Validate();
      isLocked = true;
    }

    /// <summary>
    /// Unlocks the object.
    /// Sets <see cref="IsLocked"/> to <see langword="false"/>.
    /// </summary>
    protected void Unlock()
    {
      isLocked = false;
    }

    #endregion

    /// <inheritdoc/>
    public abstract void Validate();

    #region Clone method implementation

    /// <inheritdoc/>
    public virtual object Clone()
    {
      ConfigurationSectionBase clone = CreateClone();
      clone.Clone(this);
      return clone;
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// Used by <see cref="Clone"/> method implementation.
    /// </summary>
    /// <returns>New instance of this class.</returns>
    protected abstract ConfigurationSectionBase CreateClone();

    /// <summary>
    /// Copies the properties from the <paramref name="source"/>
    /// configuration to this one.
    /// Used by <see cref="Clone"/> method implementation.
    /// </summary>
    /// <param name="source">The configuration to copy properties from.</param>
    protected virtual void Clone(ConfigurationSectionBase source)
    {
      // Does nothing in this class.
    }

    #endregion
  }
}