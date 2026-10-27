// Copyright (C) 2013-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Xtensive.Orm.Weaver
{
  public static class Extensions
  {
    public static bool HasAttribute(this ICustomAttributeProvider target, string fullName)
    {
      var comparer = WeavingHelper.TypeNameComparer;
      return target.CustomAttributes.Any(a => comparer.Equals(a.AttributeType.FullName, fullName));
    }

    public static bool HasPublicKeyToken(this AssemblyNameReference reference, IList<byte> expectedToken)
    {
      var tokenToCheck = reference.PublicKeyToken;
      return tokenToCheck!=null
        && tokenToCheck.Length==expectedToken.Count
        && tokenToCheck.SequenceEqual(expectedToken);
    }

    public static bool HasReferenceTo(this ModuleDefinition module, string assemblyName, IList<byte> publicKeyToken)
    {
      return module.AssemblyReferences.Any(r => r.Name==assemblyName && r.HasPublicKeyToken(publicKeyToken));
    }

    public static bool Remove<T>(this Collection<T> items, string name)
      where T : MemberReference
    {
      var index = items.IndexOf(name);
      if (index < 0)
        return false;
      items.RemoveAt(index);
      return true;
    }

    public static int IndexOf<T>(this Collection<T> items, string name)
      where T : MemberReference
    {
      var comparer = WeavingHelper.TypeNameComparer;

      for (int i = 0; i < items.Count; i++)
        if (comparer.Equals(items[i].Name, name))
          return i;

      return -1;
    }

    public static TypeReference StripGenericParameters(this TypeReference type)
    {
      if (type.IsGenericInstance)
        type = type.GetElementType();
      return type;
    }

    internal static FieldReference GetBackingField(this PropertyDefinition property)
    {
      // works for unprocessed properties

      MethodDefinition searchTarget;
      OpCode targetCode;
      if (property.GetMethod is not null) {
        searchTarget = property.GetMethod;
        targetCode = OpCodes.Ldfld;
      }
      else if (property.SetMethod is not null) {
        searchTarget = property.SetMethod;
        targetCode = OpCodes.Stfld;
      }
      else {
        return null;
      }

      foreach(var inst in searchTarget.Body.Instructions) {
        if (inst.OpCode == targetCode && inst?.Operand is FieldReference fieldRef) {
          return fieldRef;
        }
      }
      return null;
    }

    internal static bool IsBaseConstructorCall(this Instruction instruction)
    {
      if (instruction.OpCode.Code is not Code.Call)
        return false;
      if (instruction.Operand is not MethodReference method)
        return false;
      if (method.Name != WellKnown.Constructor)
        return false;
      return true;
    }
  }
}