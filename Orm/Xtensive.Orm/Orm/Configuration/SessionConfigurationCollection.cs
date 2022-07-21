// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.05

using System;
using System.Collections.Generic;
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
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

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
          if (Comparer.Equals(item.Name, name))
            return item;
        return IsLocked ? Default : null;
      }
    }

    /// <inheritdoc/>
    public override void Add(SessionConfiguration item)
    {
      EnsureItemIsValid(item);
      base.Add(item);
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<SessionConfiguration> items)
    {
      EnsureNotLocked();
      foreach (var item in items) {
        Add(item);
      }
    }

    private void EnsureItemIsValid(SessionConfiguration item)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(item.Name, "SessionConfiguration.Name");
      var current = this[item.Name];
      if (current != null)
        throw new InvalidOperationException(string.Format(Strings.ExConfigurationWithXNameAlreadyRegistered, current.Name));
    }

    #region Equality members

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj)
        || obj is SessionConfigurationCollection other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(SessionConfigurationCollection obj)
    {
      if (obj is null)
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
      foreach (var item in this.Where(item => !item.IsLocked))
        item.Lock(recursive);
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
  }
}