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

      Assert.That(parameter.Name, Is.EqualTo(parameterName));
      Assert.Throws<NotSupportedException>(() => _ = parameter.Value);

      var firstContext = new ParameterContext();
      firstContext.SetValue(parameter, 10);
      Assert.That(firstContext.GetValue(parameter), Is.EqualTo(10));
      firstContext.SetValue(parameter, 15);
      Assert.That(firstContext.GetValue(parameter), Is.EqualTo(15));

      var secondContext = new ParameterContext(firstContext);
      Assert.That(secondContext.GetValue(parameter), Is.EqualTo(15));
      secondContext.SetValue(parameter, 20);
      Assert.That(secondContext.GetValue(parameter), Is.EqualTo(20));

      Assert.That(firstContext.GetValue(parameter), Is.EqualTo(15));
      firstContext.SetValue(parameter, 25);
      Assert.That(firstContext.GetValue(parameter), Is.EqualTo(25));
      Assert.That(secondContext.GetValue(parameter), Is.EqualTo(20));
    }
  }
}
