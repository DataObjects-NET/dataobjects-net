// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.10

using System;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Core
{
  /// <summary>
  /// A unique identifier of a location.
  /// </summary>
  [Serializable]
  public sealed class Location : IEquatable<Location>
  {
    private const string ToStringFormat = "[{0}] {1}";

    /// <summary>
    /// Gets the provider for this location.
    /// </summary>
    public string Provider { get; private set; }

    /// <summary>
    /// Gets the resource.
    /// </summary>
    public string Resource { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Provider, Resource);
    }
    
    #region GetHashCode, Equals, !=, ==

    /// <inheritdoc/>
    public bool Equals(Location other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Provider, Provider) && Equals(other.Resource, Resource);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Location))
        return false;
      return Equals((Location) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = Provider.GetHashCode();
        result = (result * 397) ^ Resource.GetHashCode();
        return result;
      }
    }

    /// <see cref="ClassDocTemplate.OperatorEq"/>
    public static bool operator ==(Location left, Location right)
    {
      return Equals(left, right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq"/>
    public static bool operator !=(Location left, Location right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <summary>
    /// Builds a <see cref="Location"/> from <see cref="UrlInfo"/>.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>Location that corrensponds to <paramref name="url"/>.</returns>
    public static Location FromUrl(UrlInfo url)
    {
      ArgumentValidator.EnsureArgumentNotNull(url, "url");

      var resource = new StringBuilder();
      if (!string.IsNullOrEmpty(url.Host)) {
        resource.Append(url.Host);
        if (url.Port!=0) {
          resource.Append(":");
          resource.Append(url.Port);
        }
      }
      resource.Append("/");
      if (!string.IsNullOrEmpty(url.Resource))
        resource.Append(url.Resource);
      
      return new Location(url.Protocol, resource.ToString());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="resource">The address.</param>
    public Location(string provider, string resource)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(resource, "resource");
      
      Provider = provider.ToLowerInvariant();
      Resource = resource;
    }
  }
}