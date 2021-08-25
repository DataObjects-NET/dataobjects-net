using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners
{
  public abstract class TestRunnerBase
  {
    protected abstract string ThisClassName { get; }

    protected static string ComposeKey(string s1, string s2, string s3)
    {
      return s1 + s2 + s3;
    }

    protected void InitVariables(string methodName, out ParameterContainer var1, out ParameterContainer var2, out ParameterContainer var3)
    {
      var1 = new ParameterContainer() {
        BaseNameField = ThisClassName + methodName + "BaseField1",
        BaseNameProp = ThisClassName + methodName + "BaseProp1",
        NameField = ThisClassName + methodName + "Field1",
        NameProp = ThisClassName + methodName + "Prop1",
      };
      var2 = new ParameterContainer() {
        BaseNameField = ThisClassName + methodName + "BaseField2",
        BaseNameProp = ThisClassName + methodName + "BaseProp2",
        NameField = ThisClassName + methodName + "Field2",
        NameProp = ThisClassName + methodName + "Prop2",
      };
      var3 = new ParameterContainer() {
        BaseNameField = ThisClassName + methodName + "BaseField3",
        BaseNameProp = ThisClassName + methodName + "BaseProp3",
        NameField = ThisClassName + methodName + "Field3",
        NameProp = ThisClassName + methodName + "Prop3",
      };
    }
  }
}
