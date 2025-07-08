using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base type for all NUnit attributes for test methods
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public abstract class NUnitTestCustomAttribute : Attribute, ITestAction
  {
    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
      OnBeforeTestCheck(test);
    }

    public void AfterTest(ITest test)
    {
      OnAfterTestCheck(test);
    }

    protected virtual void OnBeforeTestCheck(ITest test)
    {

    }

    protected virtual void OnAfterTestCheck(ITest test)
    {

    }
  }
}
