// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.27

using System;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Building.Builders
{
  internal static class ValueTypeBuilder
  {
    public static object AdjustValue(FieldDef targetField, object value)
    {
      if (targetField.ValueType.IsAssignableFrom(value.GetType()))
        return value;
      var valueType = targetField.ValueType.StripNullable();
      if (valueType==typeof (Guid)) {
        Guid guid;
        try {
          guid = new Guid((string) value);
        }
        catch (FormatException) {
          throw FailToParseValue(targetField, value);
        }
        return guid;
      }
      if (valueType==typeof (TimeSpan)) {
        TimeSpan timespan;
        if (value is string && !TimeSpan.TryParse((string) value, out timespan))
          throw FailToParseValue(targetField, value);
        var ticks = (long) Convert.ChangeType(value, typeof (long));
        timespan = TimeSpan.FromTicks(ticks);
        return timespan;
      }
      return Convert.ChangeType(value, valueType);
    }

    private static DomainBuilderException FailToParseValue(FieldDef targetField, object value)
    {
      return new DomainBuilderException(String.Format(Strings.ExUnableToParseValueXForFieldY, value, targetField.Name));
    }
  }
}