// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class SessionElement : ConfigurationElement
  {
    private const string UserNameElementName = "userName";
    private const string CacheSizeElementName = "cacheSize";

    /// <summary>
    /// Gets user name to authenticate.
    /// </summary>
    [ConfigurationProperty(UserNameElementName, IsRequired = false)]
    public string UserName
    {
      get { return (string) this[UserNameElementName]; }
      set { this[UserNameElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the size of the session cache.
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, IsRequired = false)]
    public int CacheSize
    {
      get { return (int) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    public SessionConfiguration AsSessionConfiguration()
    {
      var result = new SessionConfiguration{
          UserName = UserName,
          CacheSize = CacheSize
        };
      return result;
    }
  }
}