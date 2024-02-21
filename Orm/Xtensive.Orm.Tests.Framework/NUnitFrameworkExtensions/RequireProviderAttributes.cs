using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base attribute for test storage requirement
  /// </summary>
  public abstract class RequireProvderAttribute : Attribute, ITestAction
  {
    private Version minVersion = null;
    private Version maxVersion = null;

    /// <summary>
    /// Gets or sets Minimal version that required for test.
    /// If not set, version check is not applied
    /// </summary>
    public virtual string MinVersion
    {
      get { return (minVersion == null) ? null : minVersion.ToString(); }
      set {
        if (value == null)
          minVersion = null;
        else if (!Version.TryParse(value, out minVersion))
          throw new ArgumentException("Not a version string", nameof(value));
      }
    }

    /// <summary>
    /// Gets or sets Maximal version that required for test.
    /// If not set, version check is not applied.
    /// </summary>
    public virtual string MaxVersion
    {
      get { return (maxVersion == null) ? null : maxVersion.ToString(); }
      set {
        if (value == null)
          maxVersion = null;
        else if (!Version.TryParse(value, out maxVersion))
          throw new ArgumentException("Not a version string", nameof(value));
      }
    }

    protected abstract StorageProvider RequiredProviders { get; }

    public ActionTargets Targets => ActionTargets.Test;

    public void AfterTest(ITest test)
    {
      Require.ProviderIs(RequiredProviders);
      if (minVersion != null)
        Require.ProviderVersionAtLeast(minVersion);
      if (maxVersion != null)
        Require.ProviderVersionAtMost(maxVersion);
    }

    public void BeforeTest(ITest test) { }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequireSqlServerAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.SqlServer;
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequirePostgreSqlAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.PostgreSql;
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequireMySqlAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.MySql;
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequireFirebirdAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.Firebird;
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequireOracleSqlAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.Oracle;
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequireSqliteAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.Sqlite;
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class RequireSeveralProvidersAttribute : RequireProvderAttribute
  {
    private readonly StorageProvider providers;

    protected override StorageProvider RequiredProviders => providers;

    public override string MinVersion
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override string MaxVersion
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public RequireSeveralProvidersAttribute(StorageProvider allowedProviders)
    {
      providers = allowedProviders;
    }
  }
}
