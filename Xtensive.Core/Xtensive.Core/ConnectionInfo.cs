// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core
{
  /// <summary>
  /// A wrapper representing connection information.
  /// Connection can be specified by either <see cref="ConnectionString"/> or <see cref="ConnectionUrl"/>.
  /// </summary>
  public sealed class ConnectionInfo : IEquatable<ConnectionInfo>
  {
    private const string ToStringFormat = "Provider Name={0};{1}";

    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    public string ProviderName { get; private set; }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    public string ConnectionString { get; private set; }

    /// <summary>
    /// Gets the connection URL.
    /// </summary>
    public UrlInfo ConnectionUrl { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return ConnectionUrl!=null
        ? ConnectionUrl.ToString()
        : string.Format(ToStringFormat, ProviderName, ConnectionString);
    }

    #region GetHashCode, Equals, ==, !=

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (ProviderName!=null ? ProviderName.GetHashCode() : 0);
        result = (result * 397) ^ (ConnectionString!=null ? ConnectionString.GetHashCode() : 0);
        result = (result * 397) ^ (ConnectionUrl!=null ? ConnectionUrl.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals((ConnectionInfo) obj);
    }

    /// <inheritdoc/>
    public bool Equals(ConnectionInfo other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.ProviderName, ProviderName)
        && Equals(other.ConnectionString, ConnectionString)
        && Equals(other.ConnectionUrl, ConnectionUrl);
    }

    /// <inheritdoc/>
    public static bool operator ==(ConnectionInfo left, ConnectionInfo right)
    {
      return Equals(left, right);
    }

    /// <inheritdoc/>
    public static bool operator !=(ConnectionInfo left, ConnectionInfo right)
    {
      return !Equals(left, right);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="providerName">A value for <see cref="ProviderName"/>.</param>
    /// <param name="connectionString">A value for <see cref="ConnectionString"/>.</param>
    public ConnectionInfo(string providerName, string connectionString)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(providerName, "providerName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionString, "connectionString");

      ConnectionString = connectionString;
      ProviderName = providerName.ToLowerInvariant();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="connectionUrl">A value for <see cref="ConnectionUrl"/>.</param>
    public ConnectionInfo(UrlInfo connectionUrl)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionUrl, "connectionUrl");

      ConnectionUrl = connectionUrl;
      ProviderName = connectionUrl.Protocol.ToLowerInvariant();
    }
  }
}