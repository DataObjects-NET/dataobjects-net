using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Ignores test if test run is in Github Action environment
  /// </summary>
  public sealed class IgnoreIfGithubActionsAttribute : NUnitTestCustomAttribute
  {
    protected override void OnBeforeTestCheck(ITest test)
    {
      if (TestInfo.IsGitHubActions) {
        throw new IgnoreException("Ignored due to run on Github Actions. Test is always failing.");
      }
    }
  }
}