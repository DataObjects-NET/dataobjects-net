using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.CodeDom;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tuples.Internals
{
  internal class TupleInterfaceInfo
  {
    public readonly TupleInfo TupleInfo;
    public readonly Type InterfaceType;
    public readonly Type FieldType;
    public readonly bool IsForValueType;
    public readonly bool IsForNullableType;


    // Constructors

    public TupleInterfaceInfo(TupleInfo tupleInfo, Type fieldType)
    {
      TupleInfo = tupleInfo;
      IsForValueType = fieldType.IsValueType;
      IsForNullableType = fieldType.IsNullable();
      if (IsForNullableType)
        FieldType = fieldType.GetGenericArguments()[0];
      else
        FieldType = fieldType;
      InterfaceType = typeof (ITupleFieldAccessor<>).MakeGenericType(fieldType);
      tupleInfo.Interfaces.Add(fieldType, this);
    }
  }
}
