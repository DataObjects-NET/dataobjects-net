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

    protected override void OnBeforeTestCheck(ITest test)
    {
      if (TestInfo.IsGitHubActions) {
        throw new IgnoreException(
          string.IsNullOrEmpty(Reason)
            ? "Ignored due to run on Github Actions. Test is always failing."
            : $"Ignored due to run on Github Actions. {Reason}");
      }
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