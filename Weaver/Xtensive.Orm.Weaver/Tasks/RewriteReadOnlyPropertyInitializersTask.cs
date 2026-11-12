// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class RewriteReadOnlyPropertyInitializersTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly MethodDefinition constructor;
    private readonly Dictionary<MetadataToken, PropertyInfo> persistentPropsBackingFields;

    public override ActionResult Execute(ProcessorContext context)
    {
      if (!constructor.HasBody)
        return ActionResult.Success;

      var il = constructor.Body.GetILProcessor();
      var instructions = constructor.Body.Instructions;

      var fieldPreInitializations = FindReadOnlyFieldsInitializations(instructions, out var lastFieldInitPosition, out var ctorPosition);
      if (fieldPreInitializations.Count == 0)
        return ActionResult.Success;

      var requireRewrite = new Dictionary<MetadataToken, PropertyInfo>();
      foreach (var bF in fieldPreInitializations) {
        if (persistentPropsBackingFields.TryGetValue(bF.MetadataToken, out var prop) && prop.IsReadOnly) {
          requireRewrite.Add(bF.MetadataToken, prop);
        }
      }

      if (requireRewrite.Count == 0)
        return ActionResult.Success;

      if (requireRewrite.Values.Any(p => p.IsKey)) {
        context.Logger.Write(MessageCode.WarningKeyFieldSettingDetectedInsideConstructor, $"Type: '{type.FullName}' (from '{type.Namespace}')");
        return ActionResult.Failure;
      }

      var start = ctorPosition + 1;
      var lenght = lastFieldInitPosition - ctorPosition;

      // caching current instructions is required
      var processibleWindow = instructions.Skip(start).Take(lenght).ToArray();

      for (int i = 0, count = processibleWindow.Length; i < count; i++) {
        var inst = processibleWindow[i];

        // Within type we can search by tokens
        if (inst.OpCode == OpCodes.Stfld && inst.Operand is FieldReference fieldRef
          && requireRewrite.TryGetValue(fieldRef.MetadataToken, out var owningProp)) {

          // Usual pattern of stack state for setting a field value is 'this' and the value to set,
          // but the method call like SetFieldValue<T>("SomeProperty", value) requires three parameters
          // with the extra one - in between the two.
          // We have to re-arrange some values on stack for method to pickup
          // so we use intermediate variable

          var propertyType = owningProp.Definition.PropertyType;

          var accessor = GetAccessor(context, propertyType,
            context.References.PersistentSetters,
            context.References.PersistentSetterDefinition);

          var valueType = propertyType;
          var localVar = new VariableDefinition(valueType);
          constructor.Body.Variables.Add(localVar);

          // Re-arranging the stack
          il.InsertBefore(inst, il.Create(OpCodes.Stloc, localVar));
          il.InsertBefore(inst, il.Create(OpCodes.Ldstr, owningProp.PersistentName ?? owningProp.Name));
          il.InsertBefore(inst, il.Create(OpCodes.Ldloc, localVar));

          var newCall = il.Create(OpCodes.Call, accessor);
          il.Replace(inst, newCall);
        }
      }

      return ActionResult.Success;
    }

    private MethodReference GetAccessor(ProcessorContext context,
      TypeReference propertyType,
      IDictionary<TypeIdentity, MethodReference> registry, MethodReference definition)
    {
      var identity = new TypeIdentity(propertyType);
      if (!registry.TryGetValue(identity, out var result)) {
        var accessorTemplate = new GenericInstanceMethod(definition);
        accessorTemplate.GenericArguments.Add(propertyType);
        result = context.TargetModule.ImportReference(accessorTemplate);
        registry.Add(identity, result);
      }
      return result;
    }

    private static IReadOnlyList<FieldReference> FindReadOnlyFieldsInitializations(Mono.Collections.Generic.Collection<Instruction> instructions,
      out int lastFieldPosition, out int ctorPosition)
    {
      lastFieldPosition = -1;
      ctorPosition = -1;
      if (instructions.Count == 0)
        return Array.Empty<FieldReference>();

      var initializedBackingFields = new List<FieldReference>();

      var index = 0;
      var ctorReached = false;
      var current = instructions.FirstOrDefault();
      while (current is not null) {
        var isBaseCtor = current.IsBaseConstructorCall();
        if (isBaseCtor) {
          ctorReached = true;
          ctorPosition = index;
        }
        if (!ctorReached) {
          current = current.Next;
          index++;
          continue;
        }

        if (current.OpCode == OpCodes.Stfld && current?.Operand is FieldReference fieldRef) {
          initializedBackingFields.Add(fieldRef);
          lastFieldPosition = index;
        }
        current = current.Next;
        index++;
      }

      return initializedBackingFields;
    }

    public RewriteReadOnlyPropertyInitializersTask(TypeDefinition type, MethodDefinition constructor, Dictionary<FieldReference, PropertyInfo> backingFieldMap)
    {
      this.type = type ?? throw new ArgumentNullException(nameof(type));
      this.constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));

      ArgumentNullException.ThrowIfNull(backingFieldMap);

      persistentPropsBackingFields = backingFieldMap.ToDictionary(
        static kv => kv.Key.MetadataToken,
        static kv => kv.Value);
    }
  }
}