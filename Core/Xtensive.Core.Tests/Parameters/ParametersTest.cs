// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.18

using System;
using NUnit.Framework;
using Xtensive.Parameters;
using Xtensive.Testing;

namespace Xtensive.Tests.Parameters
{
  [TestFixture]
  public class ParametersTest
  {    
    [Test]
    public void CombinedTest()
    {
      var parameter = new Parameter<int>();

      Assert.AreEqual(null, parameter.ExpectedValue);
      Assert.IsFalse(parameter.IsExpectedValueSet);

      parameter = new Parameter<int>(1);

      Assert.AreEqual(1, parameter.ExpectedValue);

      AssertEx.Throws<Exception>(delegate {
        parameter.Value = 5;
      });

      var firstContext = new ParameterContext();

      using (firstContext.Activate()) {        

        AssertEx.Throws<InvalidOperationException>(delegate {
          int i = parameter.Value;
        });

        parameter.Value = 10;
        Assert.AreEqual(10, parameter.Value);

        using (ParameterContext.ExpectedValues.Activate()) {
          Assert.AreEqual(1, parameter.Value);
          AssertEx.ThrowsInvalidOperationException(() => parameter.Value = 1);
        }

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

      AssertEx.Throws<Exception>(delegate {
        int i = parameter.Value;
      });
    }
  }
}