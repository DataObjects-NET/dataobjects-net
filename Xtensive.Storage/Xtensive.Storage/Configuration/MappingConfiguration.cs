// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.30

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Configuration
{
  [Serializable]
  public class MappingConfiguration : ConfigurationBase,
    IEquatable<MappingConfiguration>
  {
    private string assembly;
    private string type;

    /// <summary>
    /// Gets or sets the assembly of mapping.
    /// </summary>
    public string Assembly
    {
      get { return assembly; }
      set {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "Assembly");
        assembly = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of mapping.
    /// </summary>
    public string Type
    {
      get { return type; }
      set { 
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "Type");
        type = value;
      }
    }

    public override void Validate()
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(Type, "Type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(Assembly, "Assembly");
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      var clone = new MappingConfiguration();
      clone.Clone(this);
      return clone;
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      var configuration = (MappingConfiguration)source;
      Assembly = configuration.Assembly;
      Type = configuration.Type;
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(MappingConfiguration obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return
        obj.Assembly == Assembly &&
          obj.Type == Type;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != typeof(MappingConfiguration))
        return false;
      return Equals((MappingConfiguration)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = Assembly.GetHashCode();
        result = 29 * result + Type.GetHashCode();
        return result;
      }
    }

    #endregion

        /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Assembly = {0}, Type = {1}", Assembly, Type);
    }
  }
}