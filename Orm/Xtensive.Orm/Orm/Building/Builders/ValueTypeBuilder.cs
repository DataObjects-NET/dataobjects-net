// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.27

using System;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Definitions;


namespace Xtensive.Orm.Building.Builders
{
  internal static class ValueTypeBuilder
  {
    public static object AdjustValue(FieldDef targetField, object value)
    {
      var fieldType = targetField.ValueType;

      if (fieldType.IsAssignableFrom(value.GetType()))
        return value;

      // We can't do anything here cause we don't know the structure of referenced Entity's key
      if (typeof(IEntity).IsAssignableFrom(fieldType))
        return value;

      return AdjustValue(targetField.Name, fieldType, value);
    }

    public static object AdjustValue(Model.FieldInfo targetField, Type targetType, object value)
    {
      return AdjustValue(targetField.Name, targetType, value);
    }

    private static object AdjustValue(string fieldName, Type fieldType, object value)
    {
      var valueType = fieldType.StripNullable();

      if (valueType==typeof (Guid)) {
        Guid guid;
        try {
          guid = new Guid((string) value);
        }
        catch (FormatException) {
          throw FailToParseValue(fieldName, value);
        }
        return guid;
      }

      if (valueType==typeof (TimeSpan)) {
        TimeSpan timespan;
        if (value is string && !TimeSpan.TryParse((string) value, out timespan))
          throw FailToParseValue(fieldName, value);
        var ticks = (long) Convert.ChangeType(value, typeof (long));
        timespan = TimeSpan.FromTicks(ticks);
        return timespan;
      }

      return Convert.ChangeType(value, valueType);
    }

    private static DomainBuilderException FailToParseValue(string fieldName, object value)
    {
      return new DomainBuilderException(String.Format(Strings.ExUnableToParseValueXForFieldY, value, fieldName));
    }
  }
}