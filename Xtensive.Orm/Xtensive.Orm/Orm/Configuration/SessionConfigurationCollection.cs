// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.05

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="SessionConfiguration"/> collection.
  /// </summary>
  [Serializable]
  public class SessionConfigurationCollection : CollectionBaseSlim<SessionConfiguration>, 
    ICloneable
  {
    private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

    private SessionConfiguration @default;
    private SessionConfiguration system;
    private SessionConfiguration service;
    private SessionConfiguration keyGenerator;

    ///<summary>
    /// Gets the default session configuration.
    ///</summary>
    public SessionConfiguration Default
    {
      get { return GetConfiguration(WellKnown.Sessions.Default, @default); }
    }

    ///<summary>
    /// Gets the system session configuration.
    ///</summary>
    public SessionConfiguration System
    {
      get { return GetConfiguration(WellKnown.Sessions.System, system); }
    }

    ///<summary>
    /// Gets the service session configuration.
    ///</summary>
    public SessionConfiguration Service
    {
      get { return GetConfiguration(WellKnown.Sessions.Service, service); }
    }

    ///<summary>
    /// Gets the key generator session configuration.
    ///</summary>
    public SessionConfiguration KeyGenerator
    {
      get { return GetConfiguration(WellKnown.Sessions.KeyGenerator, keyGenerator); }
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
        return IsLocked ? Default : null;
      }
    }

    /// <inheritdoc/>
    public override void Insert(int index, SessionConfiguration item)
    {
      EnsureItemIsValid(item);
      base.Insert(index, item);
    }

    /// <inheritdoc/>
    public override void Add(SessionConfiguration item)
    {
      EnsureItemIsValid(item);
      base.Add(item);
    }

    private void EnsureItemIsValid(SessionConfiguration item)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(item.Name, "SessionConfiguration.Name");
      var current = this[item.Name];
      if (current != null)
        throw new InvalidOperationException(string.Format(Resources.Strings.ExConfigurationWithXNameAlreadyRegistered, current.Name));
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
        result = (result * 397) ^ (KeyGenerator!=null ? KeyGenerator.GetHashCode() : 0);
        return result;
      }
    }
 
    #endregion

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      @default = BuildConfiguration(WellKnown.Sessions.Default);
      system = BuildConfiguration(WellKnown.Sessions.System);
      service = BuildConfiguration(WellKnown.Sessions.Service);
      keyGenerator = BuildConfiguration(WellKnown.Sessions.KeyGenerator);
      foreach (var item in this.Where(item => !item.IsLocked)) {
        // ApplyDefaultSettings(item);
        item.Lock(recursive);
      }
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public object Clone()
    {
      var result = new SessionConfigurationCollection();
      foreach (var configuration in this)
        result.Add(configuration.Clone());
      return result;
    }

    private SessionConfiguration GetConfiguration(string name, SessionConfiguration fallback)
    {
      return !IsLocked ? this[name] : fallback;
    }

    private SessionConfiguration BuildConfiguration(string name)
    {
      var result = this[name];
      if (result!=null)
        return result;

      result = new SessionConfiguration(name);
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
      if (config.CacheType != Default.CacheType && config.CacheType == SessionCacheType.Default)
        config.CacheType = Default.CacheType;
      if (config.DefaultIsolationLevel != Default.DefaultIsolationLevel
        && config.DefaultIsolationLevel == SessionConfiguration.DefaultDefaultIsolationLevel)
        config.DefaultIsolationLevel = Default.DefaultIsolationLevel;
    }
  }
}