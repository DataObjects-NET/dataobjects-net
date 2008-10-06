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
    private const string NameElementName = "name";
    private const string UserNameElementName = "userName";
    private const string PasswordElementName = "password";
    private const string CacheSizeElementName = "cacheSize";
    private const string OptionsElementName = "options";

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
    /// Gets password to authenticate.
    /// </summary>
    [ConfigurationProperty(PasswordElementName, IsRequired = false)]
    public string Password
    {
      get { return (string)this[PasswordElementName]; }
      set { this[PasswordElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the size of the session cache. Default value is <see cref="SessionConfiguration.DefaultCacheSize"/>.
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, IsRequired = false, DefaultValue = SessionConfiguration.DefaultCacheSize)]
    public int CacheSize
    {
      get { return (int) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the session options. Default value is <see cref="SessionOptions.Default"/>.
    /// </summary>
    [ConfigurationProperty(OptionsElementName, IsRequired = false, DefaultValue = SessionOptions.Default)]
    public SessionOptions Options
    {
      get { return (SessionOptions) this[OptionsElementName]; }
      set { this[OptionsElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the session name. Default value is <see cref="string.Empty"/>.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, DefaultValue = "")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    public SessionConfiguration AsSessionConfiguration()
    {
      var result = new SessionConfiguration{
          UserName = UserName,
          CacheSize = CacheSize,
          Password = Password,
        };
      return result;
    }
  }
}