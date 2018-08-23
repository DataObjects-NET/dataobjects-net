// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.06.23

namespace Xtensive.Orm.Weaver
{
  /// <summary>
  /// Property checker for Non-CSharp languages.
  /// </summary>
  internal sealed class VbPropertyChecker : IPersistentPropertyChecker
  {
    ///<inheritdoc/>
    public bool HasSkippedProperties {get { return false; }}

    ///<inheritdoc/>
    public bool ShouldProcess(PropertyInfo property, ProcessorContext context)
    {
      return property.IsAutomatic;
    }
  }
}