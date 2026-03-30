// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using NUnit.Framework;
using Xtensive.Conversion;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class ConverterProviderTest
  {
    [Test]
    public void ConverterTest()
    {
      IAdvancedConverterProvider provider = AdvancedConverterProvider.Default;
      AdvancedConverter<int, string> intStringAdvancedConverter = provider.GetConverter<int, string>();
      Assert.That(intStringAdvancedConverter.Implementation.GetType().Name, Is.EqualTo("Int32AdvancedConverter"));
      Assert.That(!(intStringAdvancedConverter.IsRough), Is.True);
      Assert.That(intStringAdvancedConverter.IsRough, Is.False);
      int intValue = 1234;
      string intToStringValue = intStringAdvancedConverter.Convert(intValue);
      TestLog.Info($"IntToString: {intValue} {intToStringValue}");
      Assert.That(intToStringValue, Is.EqualTo(intValue.ToString()));
    }

    [Test]
    public void CustomConverterTest()
    {
      IAdvancedConverterProvider provider = new TestConverterProvider();
      AdvancedConverter<string, int> customStringToIntAdvancedConverter = provider.GetConverter<string, int>();
      AdvancedConverter<int, string> customIntToStringAdvancedConverter = provider.GetConverter<int, string>();
      Assert.That(customStringToIntAdvancedConverter.Implementation.GetType(), Is.EqualTo(typeof(StringAdvancedConverter)));
      Assert.That(customIntToStringAdvancedConverter.Implementation.GetType(), Is.EqualTo(typeof(StringAdvancedConverter)));
    }
  }
}