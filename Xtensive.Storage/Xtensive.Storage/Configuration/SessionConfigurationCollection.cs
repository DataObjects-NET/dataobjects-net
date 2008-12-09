// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.05

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Configuration
{
  ///<summary>
  /// The collection of <see cref="SessionConfiguration"./>
  ///</summary>
  public class SessionConfigurationCollection : CollectionBaseSlim<SessionConfiguration>
  {
    private const string defaultValue = "Default";
    private const string systemValue = "System";
    private const string serviceValue = "Service";
    private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
    private bool defaultSessionIsAdded;

    ///<summary>
    /// Gets the default session configuration.
    ///</summary>
    public SessionConfiguration Default { get; private set; }

    ///<summary>
    /// Gets the system session configuration.
    ///</summary>
    public SessionConfiguration System { get; private set; }

    ///<summary>
    /// Gets the service session configuration.
    ///</summary>
    public SessionConfiguration Service { get; private set; }


    ///<summary>
    /// Gets the element with the specified name.
    ///</summary>
    ///<param name="name">The string name of the element to get.</param>
    public SessionConfiguration this[string name]
    {
      get
      {
        foreach (var session in this)
          if (comparer.Compare(session.Name, name)==0)
            return session;
        return null;
      }
    }

    /// <inheritdoc/>
    public override void Add(SessionConfiguration item)
    {
      if (this[item.Name]!=null)
        throw new InvalidOperationException(string.Format(Strings.ExSessionWithNameXAlreadyExists, item.Name));

      var result = (SessionConfiguration) item.Clone();
      result.Name = item.Name;
      if (comparer.Compare(result.Name, defaultValue)==0) {
        Default = result;
        for (int i = 0; i < Count; i++)
          ApplyDefaultSettings(this[i]);
        ApplyDefaultSettings(System);
        ApplyDefaultSettings(Service);
        defaultSessionIsAdded = true;
      }
      else {
        if (defaultSessionIsAdded)
          ApplyDefaultSettings(result);
        if (comparer.Compare(result.Name, systemValue)==0)
          System = result;
        else if (comparer.Compare(result.Name, serviceValue)==0)
          Service = result;
      }
      base.Add(result);
    }

    #region Equality members

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (SessionConfigurationCollection))
        return false;
      return Equals((SessionConfigurationCollection) obj);
    }

    /// <inheritdoc/>
    public bool Equals(SessionConfigurationCollection obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      foreach (var configuration in obj)
        if (this[configuration.Name]==null)
          return false;
      if (!Default.Equals(obj.Default) || !System.Equals(obj.System) || !Service.Equals(obj.Service))
        return false;
      return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (Default!=null ? Default.GetHashCode() : 0);
        result = (result * 397) ^ (System!=null ? System.GetHashCode() : 0);
        result = (result * 397) ^ (Service!=null ? Service.GetHashCode() : 0);
        return result;
      }
    }

    #endregion

    private void ApplyDefaultSettings(SessionConfiguration config)
    {
      if (string.IsNullOrEmpty(config.UserName))
        config.UserName = Default.UserName;
      if (string.IsNullOrEmpty(config.Password))
        config.Password = Default.Password;
      if (config.CacheSize!=Default.CacheSize && config.CacheSize==SessionConfiguration.DefaultCacheSize)
        config.CacheSize = Default.CacheSize;
      config.Options = config.Options | Default.Options;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SessionConfigurationCollection()
    {
      Default = new SessionConfiguration {Name = defaultValue};
      System = new SessionConfiguration {Name = systemValue};
      Service = new SessionConfiguration {Name = serviceValue};
    }
  }
}