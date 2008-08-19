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
    public void CombinedTest()
    {
      Parameter<int> parameter = new Parameter<int>();

      AssertEx.Throws<InvalidOperationException>(delegate {
        parameter.Value = 5;
      });

      ParameterContext firstContext = new ParameterContext();

      using (firstContext.Activate()) {        

        AssertEx.Throws<InvalidOperationException>(delegate {
          int i = parameter.Value;
        });

        parameter.Value = 10;
        Assert.AreEqual(10, parameter.Value);

        parameter.Value = 15;
        Assert.AreEqual(15, parameter.Value);

        using (new ParameterContext().Activate()) {
          Assert.AreEqual(15, parameter.Value);

          parameter.Value = 20;
          Assert.AreEqual(20, parameter.Value);

          // Reactivating first context
          using (firstContext.Activate()) {
            Assert.AreEqual(15, parameter.Value);
            parameter.Value = 25;
          }

          Assert.AreEqual(20, parameter.Value);
        }

        Assert.AreEqual(25, parameter.Value);        
      }

      AssertEx.Throws<InvalidOperationException>(delegate {
        int i = parameter.Value;
      });
    }
  }
}