// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.08.18

using System;
using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Core.Parameters
{
  [TestFixture]
  public class ParametersTest
  {
    [Test]
    public void CombinedTest()
    {
      const string parameterName = "TestParameter";
      var parameter = new Parameter<int>(parameterName);

      Assert.AreEqual(parameterName, parameter.Name);
      Assert.Throws<NotSupportedException>(() => _ = parameter.Value);

      var firstContext = new ParameterContext();
      firstContext.SetValue(parameter, 10);
      Assert.AreEqual(10, firstContext.GetValue(parameter));
      firstContext.SetValue(parameter, 15);
      Assert.AreEqual(15, firstContext.GetValue(parameter));

      var secondContext = new ParameterContext(firstContext);
      Assert.AreEqual(15, secondContext.GetValue(parameter));
      secondContext.SetValue(parameter, 20);
      Assert.AreEqual(20, secondContext.GetValue(parameter));

      Assert.AreEqual(15, firstContext.GetValue(parameter));
      firstContext.SetValue(parameter, 25);
      Assert.AreEqual(25, firstContext.GetValue(parameter));
      Assert.AreEqual(20, secondContext.GetValue(parameter));
    }
  }
}
