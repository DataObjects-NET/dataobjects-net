// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using NUnit.Framework;
using Xtensive.Conversion;

namespace Xtensive.Tests.Conversion
{
  [TestFixture]
  public class ConverterProviderTest
  {
    [Test]
    public void ConverterTest()
    {
      IAdvancedConverterProvider provider = AdvancedConverterProvider.Default;
      AdvancedConverter<int, string> intStringAdvancedConverter = provider.GetConverter<int, string>();
      Assert.AreEqual("Int32AdvancedConverter", intStringAdvancedConverter.Implementation.GetType().Name);
      Assert.IsTrue(!(intStringAdvancedConverter.IsRough));
      Assert.IsFalse(intStringAdvancedConverter.IsRough);
      int intValue = 1234;
      string intToStringValue = intStringAdvancedConverter.Convert(intValue);
      Log.Info("IntToString: {0} {1}", intValue, intToStringValue);
      Assert.AreEqual(intValue.ToString(), intToStringValue);
    }

    [Test]
    public void CustomConverterTest()
    {
      IAdvancedConverterProvider provider = new TestConverterProvider();
      AdvancedConverter<string, int> customStringToIntAdvancedConverter = provider.GetConverter<string, int>();
      AdvancedConverter<int, string> customIntToStringAdvancedConverter = provider.GetConverter<int, string>();
      Assert.AreEqual(typeof(StringAdvancedConverter), customStringToIntAdvancedConverter.Implementation.GetType());
      Assert.AreEqual(typeof(StringAdvancedConverter), customIntToStringAdvancedConverter.Implementation.GetType());
    }
  }
}