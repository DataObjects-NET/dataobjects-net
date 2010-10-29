// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests
{
  public static class EnvironmentConfiguration
  {
    private const string StorageTypeKey = "X_STORAGE";
    private const string ForeignKeysModeKey = "X_FOREIGN_KEYS";
    private const string TypeIdKey = "X_TYPE_ID";
    private const string InheritanceSchemaKey = "X_INHERITANCE_SCHEMA";
    private const string ProviderKey = "X_PROVIDER";
    private const string ConnectionStringKey = "X_CONNECTION_STRING";
    private const string ConnectionUrlKey = "X_CONNECTION_URL";
    private const string NorthwindConnectionStringKey = "X_NORTHWIND";

    private static bool isInitialized;
    private static string storageType;
    private static ForeignKeyMode foreignKeyMode;
    private static TypeIdBehavior typeIdBehavior;
    private static InheritanceSchema inheritanceSchema;
    private static ConnectionInfo customConnectionInfo;
    private static string northwindConnectionString;

    public static string StorageType {
      get {
        EnsureIsInitialized();
        return storageType;
      }
    }
    
    public static ForeignKeyMode ForeignKeyMode {
      get {
        EnsureIsInitialized();
        return foreignKeyMode;
      }
    }
    
    public static TypeIdBehavior TypeIdBehavior {
      get {
        EnsureIsInitialized();
        return typeIdBehavior;
      }
    }
    
    public static InheritanceSchema InheritanceSchema {
      get {
        EnsureIsInitialized();
        return inheritanceSchema;
      }
    }
    
    public static ConnectionInfo CustomConnectionInfo {
      get {
        EnsureIsInitialized();
        return customConnectionInfo;
      }
    }

    public static string NorthwindConnectionString {
      get {
        EnsureIsInitialized();
        return northwindConnectionString;
      }
    }

    private static string GetEnvironmentVariable(string key)
    {
      string result = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
      if (!string.IsNullOrEmpty(result))
        return result;
      return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
    }


    private static void EnsureIsInitialized()
    {
      if (isInitialized)
        return;
      isInitialized = true;

      storageType = "memory";
      foreignKeyMode = ForeignKeyMode.Default;
      typeIdBehavior = TypeIdBehavior.Default;
      inheritanceSchema = InheritanceSchema.Default;
      customConnectionInfo = null;
      northwindConnectionString =
        "Data Source=localhost; Initial Catalog = Northwind; Integrated Security=SSPI;";

      string value;
      value = GetEnvironmentVariable(StorageTypeKey);
      if (!string.IsNullOrEmpty(value))
        storageType = value;

      value = GetEnvironmentVariable(TypeIdKey);
      if (!string.IsNullOrEmpty(value))
        typeIdBehavior = (TypeIdBehavior) Enum.Parse(typeof (TypeIdBehavior), value, true);

      value = GetEnvironmentVariable(InheritanceSchemaKey);
      if (!string.IsNullOrEmpty(value))
        inheritanceSchema = (InheritanceSchema) Enum.Parse(typeof (InheritanceSchema), value, true);

      value = GetEnvironmentVariable(ForeignKeysModeKey);
      if (!string.IsNullOrEmpty(value))
        foreignKeyMode = (ForeignKeyMode) Enum.Parse(typeof (ForeignKeyMode), value, true);

      value = GetEnvironmentVariable(ConnectionUrlKey);
      if (!string.IsNullOrEmpty(value))
        customConnectionInfo = new ConnectionInfo(value);

      value = GetEnvironmentVariable(ConnectionStringKey);
      if (!string.IsNullOrEmpty(value)) {
        var provider = GetEnvironmentVariable(ProviderKey);
        if (provider!=null)
          customConnectionInfo = new ConnectionInfo(provider, value);
      }

      value = GetEnvironmentVariable(NorthwindConnectionStringKey);
      if (!string.IsNullOrEmpty(value))
        northwindConnectionString = value;
    }
  }
}