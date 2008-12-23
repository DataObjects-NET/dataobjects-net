// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.05

using System;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Configuration
{
  ///<summary>
  /// The collection of <see cref="SessionConfiguration"./>
  ///</summary>
  [Serializable]
  public class SessionConfigurationCollection : CollectionBaseSlim<SessionConfiguration>
  {
    private const string DefaultName = "Default";
    private const string SystemName = "System";
    private const string ServiceName = "Service";
    private const string GeneratorName = "Generator";

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
          return this[DefaultName];
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
          return this[SystemName];
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
          return this[ServiceName];
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
          return this[GeneratorName];
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
      @default = BuildConfiguration(DefaultName);
      system = BuildConfiguration(SystemName);
      service = BuildConfiguration(ServiceName);
      generator = BuildConfiguration(GeneratorName);
      foreach (var item in this) {
        ApplyDefaultSettings(item);
        item.Lock(recursive);
      }
      base.Lock(recursive);
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
      if (config.DefaultIsolationLevel != Default.DefaultIsolationLevel
        && config.DefaultIsolationLevel == SessionConfiguration.DefaultIsolationLevelValue)
        config.DefaultIsolationLevel = Default.DefaultIsolationLevel;
      config.Options = config.Options | Default.Options;
    }
  }
}