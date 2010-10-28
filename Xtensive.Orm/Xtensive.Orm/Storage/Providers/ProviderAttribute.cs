// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.01.21

using System;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An attribute that must be applied to <see cref="HandlerFactory"/>
  /// to make it available for the storage.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public sealed class ProviderAttribute : Attribute, 
    IEquatable<ProviderAttribute>
  {
    /// <summary>
    /// Gets or sets the protocol the provider is responsible for.
    /// </summary>
    public string Protocol { get; private set; }

    /// <summary>
    /// Gets or sets the description of the provider.
    /// </summary>
    public string Description { get; private set; }

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(ProviderAttribute obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.Protocol, Protocol);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as ProviderAttribute);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (Protocol!=null ? Protocol.GetHashCode() : 0);
        return result;
      }
    }

    #endregion
    
    // Constructors

    public ProviderAttribute(string protocol)
    {
      Protocol = protocol;
    }

    public ProviderAttribute(string protocol, string description)
    {
      Protocol = protocol;
      Description = description;
    }
  }
}