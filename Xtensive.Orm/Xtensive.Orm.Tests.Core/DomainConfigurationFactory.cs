// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05
//

using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests
{
  public static class DomainConfigurationFactory
  {
    private const string StorageTypeKey = "X_STORAGE";
    private const string ForeignKeysModeKey = "X_FOREIGN_KEYS";
    private const string TypeIdKey = "X_TYPE_ID";
    private const string InheritanceSchemaKey = "X_INHERITANCE_SCHEMA";
    private const string ProviderKey = "X_PROVIDER";
    private const string ConnectionStringKey = "X_CONNECTION_STRING";
    private const string ConnectionUrlKey = "X_CONNECTION_URL";
    
    public static DomainConfiguration Create()
    {
      return Create(false);
    }
    
    public static DomainConfiguration Create(bool useConnectionString)
    {
      var storageType = EnvironmentConfiguration.StorageType;
      if (useConnectionString)
        storageType += "cs";
      var config = Create(storageType,
        EnvironmentConfiguration.InheritanceSchema,
        EnvironmentConfiguration.TypeIdBehavior,
        EnvironmentConfiguration.ForeignKeyMode);
      if (EnvironmentConfiguration.CustomConnectionInfo!=null)
        config.ConnectionInfo = EnvironmentConfiguration.CustomConnectionInfo;
      return config;
    }

    /// <summary>
    /// Do not use for regular tests! Use Require.ProviderIs to require specific storage.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>Configuration.</returns>
    public static DomainConfiguration CreateForCrudTest(string provider)
    {
      DisableModifiers();
      return DomainConfiguration.Load(provider);
    }

    private static DomainConfiguration Create(string protocol,
      InheritanceSchema schema, TypeIdBehavior typeIdBehavior, ForeignKeyMode foreignKeyMode)
    {
      DisableModifiers();
      var config = DomainConfiguration.Load(protocol);
      config.ForeignKeyMode = foreignKeyMode;
      if (schema!=InheritanceSchema.Default)
        InheritanceSchemaModifier.ActivateModifier(schema);
      if (typeIdBehavior!=TypeIdBehavior.Default)
        TypeIdModifier.ActivateModifier(typeIdBehavior);
      if (typeIdBehavior!=TypeIdBehavior.Default)
        TypeIdModifier.ActivateModifier(typeIdBehavior);
      return config;
    }

    private static void DisableModifiers()
    {
      ConcreteTableSchemaModifier.IsEnabled = false;
      SingleTableSchemaModifier.IsEnabled = false;
      ClassTableSchemaModifier.IsEnabled = false;
      IncludeTypeIdModifier.IsEnabled = false;
      ExcludeTypeIdModifier.IsEnabled = false;
      TypeIdModifier.IsEnabled = false;
    }
  }
}
