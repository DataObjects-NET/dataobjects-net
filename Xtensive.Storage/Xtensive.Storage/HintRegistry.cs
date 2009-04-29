// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.28

using System;
using System.Collections.Generic;
using Xtensive.Storage.Model;
using Xtensive.Storage;

namespace Xtensive.Storage
{
  // TODO: Широта мысли поражает... Только rename hints?
  // TODO: -> UpgradeContext. Надеюсь, реализуешь правильно - т.е. так, чтоб создавать его могли только мы.
  // Зачем это: для того, чтоб любой тип во время апгрейда знал, что он идет, и какая идет фаза.
  // TODO: + UpgradeScope. Надеюсь, реализуешь правильно - т.е. так, чтоб создавать его могли только мы.
  // TODO: Сделать в нем коллекцию хинтов UpgradeHintSet Hints. По аналогии с HintSet в Modelling.
  // Паттерн исп. такой: 
  // - upgradeContext.Hints.Add(new RenameTypeHint("OldName", typeof(...)) // used only to map Type.Name
  // - upgradeContext.Hints.Add(new RenameTableHint("OldName", typeof(...))
  // - upgradeContext.Hints.Add(new RenameColumnHint(typeof(...), "OldPropertyName", "NewPropertyName")
  // - upgradeContext.Hints.Add(new CopyColumnHint(typeof(...), "PropertyName", "AnotherPropertyName")
  // TODO: + UpgradeContext.Stage (enum UpgradeStage)

  /// <summary>
  /// Allows to register rename hints for model nodes.
  /// </summary>
  [Serializable]
  public class HintRegistry
  {
    private readonly DomainModel domainModel;

    internal List<MappingNodeRenameHint> Hints { get; private set; }

    /// <summary>
    /// Adds class rename hint to the registry.
    /// </summary>
    /// <param name="sourceTypeMappingName">Name of the source type.</param>
    /// <param name="resultType">Result type.</param>
    public void AddClassRenameHint(string sourceTypeMappingName, Type resultType)
    {
      AddHint(sourceTypeMappingName, domainModel.Types[resultType]);

      // TODO: Add dependent rename hints!
    }

    /// <summary>
    /// Adds field rename hint to the registry.
    /// </summary>
    /// <param name="targetType">Persistent type field belongs to.</param>
    /// <param name="sourceFieldMappingName">Name of the source field.</param>
    /// <param name="resultFieldName">Name of the result field.</param>
    public void AddFieldRenameHint(Type targetType, string sourceFieldMappingName, string resultFieldName)
    {
      AddHint(
        sourceFieldMappingName, 
        domainModel.Types[targetType].Fields[resultFieldName]);

      // TODO: Add dependent rename hints!
    }

    private void AddHint(string sourceMappingName, MappingNode resultMappingNode)
    {
      Hints.Add(
        new MappingNodeRenameHint(sourceMappingName, resultMappingNode));
    }


    // Constructors

    internal HintRegistry(DomainModel domainModel)
    {
      this.domainModel = domainModel;
      Hints = new List<MappingNodeRenameHint>();
    }
  }
}