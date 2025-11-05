using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Helps to ignore flaky tests
  /// </summary>
  public sealed class IgnoreOnGithubActionsIfFailedAttribute : IgnoreIfGithubActionAttributeBase
  {
    protected override bool CheckIfFailed => true;

    public IgnoreOnGithubActionsIfFailedAttribute(StorageProvider provider, GithubActionsEvents triggerEvent, string reason)
      : base((StorageProvider?) provider, triggerEvent, reason)
    {
    }


    public IgnoreOnGithubActionsIfFailedAttribute(GithubActionsEvents triggerEvent, string reason)
      : base(null, triggerEvent, reason)
    {
    }

    public IgnoreOnGithubActionsIfFailedAttribute(GithubActionsEvents triggerEvent)
      : base(null, triggerEvent, null)
    {
    }

    public IgnoreOnGithubActionsIfFailedAttribute(StorageProvider provider, string reason)
      : base(provider, null, reason)
    {
    }

    public IgnoreOnGithubActionsIfFailedAttribute(StorageProvider provider)
       : base(provider, null, null)
    {
    }

    public IgnoreOnGithubActionsIfFailedAttribute(string reason)
      : base(null, null, reason)
    {
    }

    public IgnoreOnGithubActionsIfFailedAttribute()
      : base(null, null, null)
    {
    }
  }

  /// <summary>
  /// Helps to ignore always failing tests.
  /// </summary>
  public sealed class IgnoreIfGithubActionsAttribute : IgnoreIfGithubActionAttributeBase
  {
    protected override bool CheckIfFailed => false;


    public IgnoreIfGithubActionsAttribute(StorageProvider provider, GithubActionsEvents triggerEvent, string reason)
      : base((StorageProvider?) provider, triggerEvent, reason)
    {
    }


    public IgnoreIfGithubActionsAttribute(GithubActionsEvents triggerEvent, string reason)
      : base(null, null, reason)
    {
    }

    public IgnoreIfGithubActionsAttribute(GithubActionsEvents triggerEvent)
      : base(null, triggerEvent, null)
    {
    }

    public IgnoreIfGithubActionsAttribute(StorageProvider provider, string reason)
      : base(provider, null, reason)
    {
    }

    public IgnoreIfGithubActionsAttribute(StorageProvider provider)
       : base(provider, null, null)
    {
    }

    public IgnoreIfGithubActionsAttribute(string reason)
      : base(null, null, reason)
    {
    }

    public IgnoreIfGithubActionsAttribute()
      : base(null, null, null)
    {
    }
  }

  public abstract class IgnoreIfGithubActionAttributeBase : NUnitTestCustomAttribute
  {
    public string Reason { get; private set; }

    public StorageProvider? Provider { get; private set; }

    public GithubActionsEvents? TriggerEvent { get; private set; }

    protected abstract bool CheckIfFailed { get; }

    protected override void OnBeforeTestCheck(ITest test)
    {
      if (TestInfo.IsGithubActions && !CheckIfFailed) {
        Check();
      }
    }

    protected override void OnAfterTestCheck(ITest test)
    {
      var resultOutcome = TestContext.CurrentContext.Result.Outcome;
      if (TestInfo.IsGithubActions && CheckIfFailed
        && (resultOutcome == ResultState.Failure || resultOutcome == ResultState.Error)) {
        Check();
      }
    }

    private void Check()
    {
      if (TestInfo.NoIgnoreOnGithubActions) {
        return;
      }
      if (Provider.HasValue && !StorageProviderInfo.Instance.CheckProviderIs(Provider.Value)) {
        return;
      }
      if (TriggerEvent.HasValue && TriggerEvent != TestInfo.GithubActionTrigger) {
        return;
      }

      throw new IgnoreException(
        string.IsNullOrEmpty(Reason)
          ? $"Ignored due to run on Github Actions. {(CheckIfFailed ? "Flaky tests failed this time" : "Test is always failing")}."
          : $"Ignored due to run on Github Actions. {Reason}");
    }


    protected IgnoreIfGithubActionAttributeBase(StorageProvider? provider, GithubActionsEvents? triggerEvent, string reason)
    {
      Provider = provider;
      TriggerEvent = triggerEvent;
      Reason = reason;
    }
  }
}