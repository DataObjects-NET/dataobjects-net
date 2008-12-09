// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System.Configuration;

namespace Xtensive.Storage.Configuration.Elements
{
  /// <summary>
  /// <see cref="Session"/> configuration element within a configuration file.
  /// </summary>
  public class SessionElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string UserNameElementName = "userName";
    private const string PasswordElementName = "password";
    private const string CacheSizeElementName = "cacheSize";
    private const string OptionsElementName = "options";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Name; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, IsKey = true, DefaultValue = "Default")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.UserName" copy="true"/>
    /// </summary>
    [ConfigurationProperty(UserNameElementName, IsRequired = false, IsKey = false)]
    public string UserName
    {
      get { return (string) this[UserNameElementName]; }
      set { this[UserNameElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Password" copy="true"/>
    /// </summary>
    [ConfigurationProperty(PasswordElementName, IsRequired = false, IsKey = false)]
    public string Password
    {
      get { return (string)this[PasswordElementName]; }
      set { this[PasswordElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.CacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, IsRequired = false, DefaultValue = SessionConfiguration.DefaultCacheSize, IsKey = false)]
    public int CacheSize
    {
      get { return (int) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Options" copy="true"/>
    /// </summary>
    [ConfigurationProperty(OptionsElementName, IsRequired = false, DefaultValue = SessionOptions.Default, IsKey = false)]
    public SessionOptions Options
    {
      get { return (SessionOptions) this[OptionsElementName]; }
      set { this[OptionsElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="SessionConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public SessionConfiguration ToNative()
    {
      var result = new SessionConfiguration {
        Name = Name,
        UserName = UserName,
        Password = Password,
        CacheSize = CacheSize,
        Options = Options
      };
      return result;
    }
  }
}