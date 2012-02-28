// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;

namespace Xtensive.Orm.Tests
{
  public static class EnvironmentConfiguration
  {
    private const string StorageKey = "X_STORAGE";
    private const string NorthwindConnectionStringKey = "X_NORTHWIND";

    private const string ConnectionUrlKeyFormat = "X_{0}_URL";
    private const string ConnectionStringKeyFormat = "X_{0}_CS";
    private const string ProviderKeyFormat = "X_{0}_PROVIDER";

    private const string DefaultStorage = "default";
    private const string DefaultNorthwindConnectionString =
      "Data Source=localhost; Initial Catalog = Northwind; Integrated Security=SSPI;";

    private static bool isInitialized;
    private static string northwindConnectionString;

    private static string storage;
    private static string connectionString;
    private static string connectionUrl;
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

    private static string GetEnvironmentVariable(string key)
    {
      string result = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
      if (!string.IsNullOrEmpty(result))
        return result;
      result = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
      if (!string.IsNullOrEmpty(result))
        return result;
      return null;
    }

    private static void EnsureIsInitialized()
    {
      if (isInitialized)
        return;
      isInitialized = true;

      storage = GetEnvironmentVariable(StorageKey) ?? DefaultStorage;
      northwindConnectionString =
        GetEnvironmentVariable(NorthwindConnectionStringKey) ?? DefaultNorthwindConnectionString;

      var name = storage.ToUpper();
      connectionUrl = GetEnvironmentVariable(string.Format(ConnectionUrlKeyFormat, name));
      connectionString = GetEnvironmentVariable(string.Format(ConnectionStringKeyFormat, name));
      provider = GetEnvironmentVariable(string.Format(ProviderKeyFormat, name));
    }
  }
}