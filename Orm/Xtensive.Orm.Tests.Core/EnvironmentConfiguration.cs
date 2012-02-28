// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xtensive.Orm.Tests
{
  public static class EnvironmentConfiguration
  {
    private const string ConfigurationKey = "X_CONFIGURATION";

    private const string StorageKey = "X_STORAGE";
    private const string NorthwindConnectionStringKey = "X_NORTHWIND";

    private const string ConnectionUrlKeyFormat = "X_{0}_URL";
    private const string ConnectionStringKeyFormat = "X_{0}_CS";
    private const string ProviderKeyFormat = "X_{0}_PROVIDER";

    private const string DefaultStorage = "default";
    private const string DefaultNorthwindConnectionString =
      "Data Source=localhost; Initial Catalog = Northwind; Integrated Security=SSPI;";

    private static bool isInitialized;
    private static Dictionary<string, string> configuration;

    private static string storage;
    private static string northwindConnectionString;
    private static string connectionUrl;
    private static string connectionString;
    private static string provider;

    public static string Storage {
      get {
        EnsureIsInitialized();
        return storage;
      }
    }

    public static string NorthwindConnectionString {
      get {
        EnsureIsInitialized();
        return northwindConnectionString;
      }
    }

    public static string ConnectionUrl {
      get {
        EnsureIsInitialized();
        return connectionUrl;
      }
    }

    public static string ConnectionString {
      get {
        EnsureIsInitialized();
        return connectionString;
      }
    }

    public static string Provider {
      get {
        EnsureIsInitialized();
        return provider;
      }
    }

    private static string GetVariable(string key)
    {
      string result = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
      if (!string.IsNullOrEmpty(result))
        return result;
      result = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
      if (!string.IsNullOrEmpty(result))
        return result;
      if (configuration!=null && configuration.TryGetValue(key, out result) && !string.IsNullOrEmpty(result))
        return result;
      return null;
    }

    private static void EnsureIsInitialized()
    {
      if (isInitialized)
        return;
      isInitialized = true;

      var configurationFile = GetVariable(ConfigurationKey);
      if (configurationFile!=null && File.Exists(configurationFile))
        LoadConfigurationFile(configurationFile);

      storage = GetVariable(StorageKey) ?? DefaultStorage;
      northwindConnectionString =
        GetVariable(NorthwindConnectionStringKey) ?? DefaultNorthwindConnectionString;

      var name = storage.ToUpper();
      connectionUrl = GetVariable(string.Format(ConnectionUrlKeyFormat, name));
      connectionString = GetVariable(string.Format(ConnectionStringKeyFormat, name));
      provider = GetVariable(string.Format(ProviderKeyFormat, name));
    }

    private static void LoadConfigurationFile(string file)
    {
      var entries =
        from line in File.ReadAllLines(file)
        let items = line.Trim().Split(new[] {'='}, 2)
        where items.Length==2
        let key = items[0].Trim()
        let value = items[1].Trim()
        where key!=string.Empty && value!=string.Empty
        select new {key, value};
      configuration = entries.ToDictionary(i => i.key, i => i.value);
    }
  }
}