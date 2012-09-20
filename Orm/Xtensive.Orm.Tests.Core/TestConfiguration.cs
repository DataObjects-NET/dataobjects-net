// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  public sealed class TestConfiguration
  {
    private const string StorageKey = "X_STORAGE";
    private const string StorageFileKey = "X_STORAGE_FILE";
    private const string ConfigurationFileKey = "X_CONFIG_FILE";
    private const string NorthwindKey = "northwind";

    private const string DefaultStorage = "default";
    private const string DefaultNorthwindConnectionString = "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True";

    private static readonly object InstanceLock = new object();
    private static TestConfiguration InstanceValue;

    private readonly Dictionary<string, string> configuration;

    public static TestConfiguration Instance
    {
      get
      {
        lock (InstanceLock) {
          if (InstanceValue==null)
            InstanceValue = new TestConfiguration();
          return InstanceValue;
        }
      }
    }

    public string Storage { get; private set; }

    public string NorthwindConnectionString { get; private set; }

    public ConnectionInfo GetConnectionInfo(string name)
    {
      var value = GetConfigurationVariable(name);
      if (value==null)
        return null;

      if (!value.StartsWith("["))
        return new ConnectionInfo(value);

      var items = value.Split(new[] {'[', ']'}, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
      if (items.Length!=2)
        throw new InvalidOperationException(string.Format("Invalid connection string format: {0}", value));
      return new ConnectionInfo(items[0], items[1]);
    }

    private string GetEnvironmentVariable(string key)
    {
      return new[] {EnvironmentVariableTarget.Process, EnvironmentVariableTarget.User, EnvironmentVariableTarget.Machine}
        .Select(target => Environment.GetEnvironmentVariable(key, target))
        .FirstOrDefault(result => !string.IsNullOrEmpty(result));
    }

    private string GetConfigurationVariable(string key)
    {
      string result;
      if (configuration.TryGetValue(key, out result) && !string.IsNullOrEmpty(result))
        return result;
      return null;
    }

    private static Dictionary<string, string> ParseConfigurationFile(string file)
    {
      var entries =
        from line in File.ReadAllLines(file)
        let items = line.Trim().Split(new[] {'='}, 2)
        where items.Length==2
        let key = items[0].Trim()
        let value = items[1].Trim()
        where key!=string.Empty && value!=string.Empty
        select new {key, value};
      return entries.ToDictionary(i => i.key, i => i.value);
    }

    private Dictionary<string, string> LoadConfiguration()
    {
      var configurationFile = GetEnvironmentVariable(ConfigurationFileKey);
      if (configurationFile!=null && File.Exists(configurationFile))
        return ParseConfigurationFile(configurationFile);
      return new Dictionary<string, string>();
    }

    private string GetStorageFromFile()
    {
      var storageFile = GetEnvironmentVariable(StorageFileKey);
      if (storageFile!=null && File.Exists(storageFile))
        return File.ReadAllLines(storageFile).Select(l => l.Trim()).FirstOrDefault();
      return null;
    }

    private TestConfiguration()
    {
      configuration = LoadConfiguration();
      Storage = GetEnvironmentVariable(StorageKey) ?? GetStorageFromFile() ?? DefaultStorage;
      NorthwindConnectionString = GetConfigurationVariable(NorthwindKey) ?? DefaultNorthwindConnectionString;
    }
  }
}