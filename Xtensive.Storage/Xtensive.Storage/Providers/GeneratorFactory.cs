// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using System;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  public abstract class GeneratorFactory : HandlerBase
  {
    public Generator CreateGenerator(HierarchyInfo hierarchy)
    {
      if (hierarchy.Fields.Count > 2)
        throw new InvalidOperationException();
      if (hierarchy.Fields.Count == 2 && !hierarchy.Fields[1].Key.IsSystem)
        throw new InvalidOperationException();

      Generator result = null;
      Type fieldType = hierarchy.Fields[0].Key.ValueType;
      TypeCode code = Type.GetTypeCode(fieldType);
      switch (code) {
      case TypeCode.SByte:
        result = CreateGenerator<SByte>(hierarchy);
        break;
      case TypeCode.Byte:
        result = CreateGenerator<Byte>(hierarchy);
        break;
      case TypeCode.Int16:
        result = CreateGenerator<Int16>(hierarchy);
        break;
      case TypeCode.UInt16:
        result = CreateGenerator<UInt16>(hierarchy);
        break;
      case TypeCode.Int32:
        result = CreateGenerator<Int32>(hierarchy);
        break;
      case TypeCode.UInt32:
        result = CreateGenerator<UInt32>(hierarchy);
        break;
      case TypeCode.Int64:
        result = CreateGenerator<Int64>(hierarchy);
        break;
      case TypeCode.UInt64:
        result = CreateGenerator<UInt64>(hierarchy);
        break;
      case TypeCode.Object:
          if (fieldType == typeof(Guid))
            result = new GuidGenerator(hierarchy);
        break;
      }
      if (result == null)
        throw new ArgumentOutOfRangeException();
      result.Handlers = Handlers;
      return result;
    }

    protected abstract Generator CreateGenerator<TFieldType>(HierarchyInfo hierarchy);
  }
}