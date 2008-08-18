// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.18

using System;
using NUnit.Framework;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Parameters
{
  [TestFixture]
  public class ParametersTest
  {    

    [Test]
    public void Test()
    {
      Parameter<int> parameter = new Parameter<int>();

      ParameterContext context = new ParameterContext();

      AssertEx.Throws<Exception>(delegate {
        parameter.Value = 5;
      });

      using (new ParameterScope(context)) {
        parameter.Value = 10;
        Assert.AreEqual(10, parameter.Value);

        parameter.Value = 15;
        Assert.AreEqual(15, parameter.Value);

        using (new ParameterScope(context)) {
          parameter.Value = 20;
          Assert.AreEqual(20, parameter.Value);
        }

        Assert.AreEqual(15, parameter.Value);
      }

      AssertEx.Throws<Exception>(delegate {
        int i = parameter.Value;
      });
    }
  }
}