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
  internal sealed class CsPropertyCheckerRelaxed : IPersistentPropertyChecker
  {
    ///<inheritdoc/>
    public bool HasSkippedProperties { get; private set; }

    ///<inheritdoc/>
    public bool ShouldProcess(PropertyInfo property, ProcessorContext context)
    {
      if (!context.ReadOnlyProperties) {
        throw new System.NotSupportedException("Current Property checker can't be used in Strict mode");
      }

      if (property.IsAutomatic)
        return true;
      var def = property.Definition;
      var getter = def.GetMethod;
      var setter = def.SetMethod;

      var baseMethod = getter ?? setter;

      // if the property has a human made body we shouldn't touch them
      if (!baseMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute))
        return false;

      // read-only properties may be alloed
      if (getter is null) {
        context.Logger.Write(MessageCode.WarningPersistentPropertyHasNoGetter, $"'{property.Name}' property of '{property.DeclaringType.FullName}' type");
        HasSkippedProperties = true;
      }

      return true;
    }
  }
}