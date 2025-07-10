using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Ignores test if test run is in Github Action environment
  /// </summary>
  public sealed class IgnoreIfGithubActionsAttribute : NUnitTestCustomAttribute
  {
    public string Reason { get; private set; }

    public StorageProvider? Provider { get; private set; }

    protected override void OnBeforeTestCheck(ITest test)
    {
      if (TestInfo.IsGitHubActions) {
        if (Provider.HasValue && !StorageProviderInfo.Instance.CheckProviderIs(Provider.Value))
          return;
        throw new IgnoreException(
          string.IsNullOrEmpty(Reason)
            ? "Ignored due to run on Github Actions. Test is always failing."
            : $"Ignored due to run on Github Actions. {Reason}");
      }
    }

    public IgnoreIfGithubActionsAttribute(StorageProvider provider, string reason)
    {
      Reason = reason;
      Provider = provider;
    }

    public IgnoreIfGithubActionsAttribute(StorageProvider provider)
    {
      Provider = provider;
    }

    public IgnoreIfGithubActionsAttribute(string reason)
    {
      Reason = reason;
    }

    public IgnoreIfGithubActionsAttribute()
    {
    }
  }
}