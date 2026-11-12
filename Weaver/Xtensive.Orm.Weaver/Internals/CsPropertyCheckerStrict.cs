// Copyright (C) 2018-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.06.23

namespace Xtensive.Orm.Weaver
{
  /// <summary>
  /// Property checker for CSharp language.
  /// </summary>
  internal sealed class CsPropertyCheckerStrict : IPersistentPropertyChecker
  {
    ///<inheritdoc/>
    public bool HasSkippedProperties { get; private set; }

    ///<inheritdoc/>
    public bool ShouldProcess(PropertyInfo property, ProcessorContext context)
    {
      if (property.IsAutomatic)
        return true;
      var def = property.Definition;
      var getter = def.GetMethod;
      var setter = def.SetMethod;

      var baseMethod = getter ?? setter;

      // if the property has a human made body we shouldn't touch them
      if (!baseMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute))
        return false;

      // if it's compiler-generated property though
      // we should check it contains both getter and setter
      // otherwise the whole idea is corrupted
      if (getter is null || setter is null) {
        context.Logger.Write(MessageCode.WarningPersistentPropertyHasNoSetterOrGetter, $"'{property.Name}' property of '{property.DeclaringType.FullName}' type");
        HasSkippedProperties = true;
        return false;
      }
      return true;
    }
  }
}