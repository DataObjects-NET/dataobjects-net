// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.05

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// <see cref="SessionConfiguration"/> collection.
  /// </summary>
  [Serializable]
  public class SessionConfigurationCollection : CollectionBaseSlim<SessionConfiguration>, 
    ICloneable
  {
    /// <summary>
    /// Default session configuration name.
    /// </summary>
    public readonly static string DefaultSessionName = "Default";
    /// <summary>
    /// System session name.
    /// </summary>
    public readonly static string SystemSessionName = "System";
    /// <summary>
    /// Service session name.
    /// </summary>
    public readonly static string ServiceSessionName = "Service";
    /// <summary>
    /// Generator session name.
    /// </summary>
    public readonly static string GeneratorSessionName = "Generator";

    private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

    private SessionConfiguration @default;
    private SessionConfiguration system;
    private SessionConfiguration service;
    private SessionConfiguration generator;

    ///<summary>
    /// Gets the default session configuration.
    ///</summary>
    public SessionConfiguration Default
    {
      get
      {
        if (!IsLocked)
          return this[DefaultSessionName];
        return @default;
      }
    }

    ///<summary>
    /// Gets the system session configuration.
    ///</summary>
    public SessionConfiguration System
    {
      get
      {
        if (!IsLocked)
          return this[SystemSessionName];
        return system;
      }
    }

    ///<summary>
    /// Gets the service session configuration.
    ///</summary>
    public SessionConfiguration Service
    {
      get
      {
        if (!IsLocked)
          return this[ServiceSessionName];
        return service;
      }
    }

    ///<summary>
    /// Gets the generator session configuration.
    ///</summary>
    public SessionConfiguration Generator
    {
      get
      {
        if (!IsLocked)
          return this[GeneratorSessionName];
        return generator;
      }
    }

    ///<summary>
    /// Gets the element with the specified name.
    ///</summary>
    ///<param name="name">The string name of the element to get.</param>
    public SessionConfiguration this[string name]
    {
      get
      {
        foreach (var item in this)
          if (comparer.Compare(item.Name, name)==0)
            return item;
        return null;
      }
    }

    #region Equality members

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var scc = obj as SessionConfigurationCollection;
      if (scc == null)
        return false;
      return Equals(scc);
    }

    /// <inheritdoc/>
    public bool Equals(SessionConfigurationCollection obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (Count != obj.Count)
        return false;
      foreach (var item2 in obj) {
        var item1 = this[item2.Name];
        if (item1==null)
          return false;
        if (!item2.Equals(item1))
          return false;
      }
      return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (Default!=null ? Default.GetHashCode() : 0);
        result = (result * 397) ^ (System!=null ? System.GetHashCode() : 0);
        result = (result * 397) ^ (Service!=null ? Service.GetHashCode() : 0);
        result = (result * 397) ^ (Generator!=null ? Generator.GetHashCode() : 0);
        return result;
      }
    }
 
    #endregion

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      @default = BuildConfiguration(DefaultSessionName);
      system = BuildConfiguration(SystemSessionName);
      service = BuildConfiguration(ServiceSessionName);
      generator = BuildConfiguration(GeneratorSessionName);
      foreach (var item in this) {
        ApplyDefaultSettings(item);
        item.Lock(recursive);
      }
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public object Clone()
    {
      var result = new SessionConfigurationCollection();
      foreach (var configuration in this)
        result.Add((SessionConfiguration) configuration.Clone());
      return result;
    }

    private SessionConfiguration BuildConfiguration(string name)
    {
      var result = this[name];
      if (result!=null)
        return result;

      result = new SessionConfiguration {Name = name};
      Add(result);
      return result;
    }

    private void ApplyDefaultSettings(SessionConfiguration config)
    {
      if (string.IsNullOrEmpty(config.UserName))
        config.UserName = Default.UserName;
      if (string.IsNullOrEmpty(config.Password))
        config.Password = Default.Password;
      if (config.CacheSize!=Default.CacheSize && config.CacheSize==SessionConfiguration.DefaultCacheSize)
        config.CacheSize = Default.CacheSize;
      if (config.CacheType != Default.CacheType && config.CacheType == SessionConfiguration.DefaultCacheType)
        config.CacheType = Default.CacheType;
      if (config.DefaultIsolationLevel != Default.DefaultIsolationLevel
        && config.DefaultIsolationLevel == SessionConfiguration.DefaultIsolationLevelValue)
        config.DefaultIsolationLevel = Default.DefaultIsolationLevel;
      config.Options = config.Options | Default.Options;
    }
  }
}