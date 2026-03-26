// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2011.09.27

using System;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Internals;


namespace Xtensive.Orm.Building.Builders
{
  internal static class ValueTypeBuilder
  {
    public static object AdjustValue(FieldDef targetField, object value)
    {
      var fieldType = targetField.ValueType;

      if (fieldType.IsInstanceOfType(value)) {
        return value;
      }

      // We can't do anything here cause we don't know the structure of referenced Entity's key
      if (WellKnownOrmInterfaces.Entity.IsAssignableFrom(fieldType)) {
        return value;
      }

      return AdjustValue(targetField.Name, fieldType, value);
    }

    public static object AdjustValue(Model.FieldInfo targetField, Type targetType, object value) =>
      AdjustValue(targetField.Name, targetType, value);

    private static object AdjustValue(string fieldName, Type fieldType, object value)
    {
      var valueType = fieldType.StripNullable();

      if (valueType == WellKnownTypes.Guid) {
        Guid guid;
        try {
          guid = new Guid((string) value);
        }
        catch (FormatException) {
          throw FailToParseValue(fieldName, value);
        }

        return guid;
      }

      if (valueType == WellKnownTypes.TimeSpan) {
        if (value is string timespanString && !TimeSpan.TryParse(timespanString, out var timespan)) {
          throw FailToParseValue(fieldName, timespanString);
        }

        var ticks = (long) Convert.ChangeType(value, WellKnownTypes.Int64);
        timespan = TimeSpan.FromTicks(ticks);
        return timespan;
      }
#if NET6_0_OR_GREATER

      if (valueType == WellKnownTypes.TimeOnly) {
        if (value is string timeOnlyString && !TimeOnly.TryParse(timeOnlyString, out var timeOnly)) {
          throw FailToParseValue(fieldName, timeOnlyString);
        }
        return timeOnly;
      }

      if (valueType == WellKnownTypes.DateOnly) {
        if (value is string dateOnlyString && !DateOnly.TryParse(dateOnlyString, out var dateOnly)) {
          throw FailToParseValue(fieldName, dateOnlyString);
        }
        return dateOnly;
      }
#endif

      return Convert.ChangeType(value, valueType);
    }

    private static DomainBuilderException FailToParseValue(string fieldName, object value) =>
      new DomainBuilderException(string.Format(Strings.ExUnableToParseValueXForFieldY, value, fieldName));
  }
}
