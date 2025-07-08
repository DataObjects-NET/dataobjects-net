using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base attribute for test storage requirement
  /// </summary>
  public abstract class RequireProvderAttribute : NUnitTestCustomAttribute
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

    protected override void OnBeforeTestCheck(ITest test)
    {
      Require.ProviderIs(RequiredProviders);
      if (minVersion != null)
        Require.ProviderVersionAtLeast(minVersion);
      if (maxVersion != null)
        Require.ProviderVersionAtMost(maxVersion);
    }
  }

  public class RequireSqlServerAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.SqlServer;
  }

  public class RequirePostgreSqlAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.PostgreSql;
  }

  public class RequireMySqlAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.MySql;
  }

  public class RequireFirebirdAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.Firebird;
  }

  public class RequireOracleSqlAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.Oracle;
  }

  public class RequireSqliteAttribute : RequireProvderAttribute
  {
    protected override StorageProvider RequiredProviders => StorageProvider.Sqlite;
  }

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
