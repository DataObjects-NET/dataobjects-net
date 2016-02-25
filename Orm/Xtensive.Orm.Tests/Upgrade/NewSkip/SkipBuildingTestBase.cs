// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.24

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Upgrade.NewSkip
{
  internal class SchemaExtractionKeeper
  {
    private readonly List<SchemaExtractionResult> initialResults = new List<SchemaExtractionResult>();
    private readonly List<SchemaExtractionResult> finalResults = new List<SchemaExtractionResult>();

    public List<SchemaExtractionResult> InitialResults
    {
      get {
        return initialResults;
      }
    }

    public List<SchemaExtractionResult> FinalResults
    {
      get {
        return finalResults;
      }
    }

    public void AddInitialResult(SchemaExtractionResult extractionResult)
    {
      initialResults.Add(extractionResult);
    }

    public void AddFinalResult(SchemaExtractionResult extractionResult)
    {
      finalResults.Add(extractionResult);
    }

    public void Clear()
    {
      initialResults.Clear();
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnComplete(Domain domain)
    {
      SchemaExtractionKeeper keeper;
      keeper = domain.Extensions.Get<SchemaExtractionKeeper>();
      if (keeper==null) {
        keeper = new SchemaExtractionKeeper();
        domain.Extensions.Set(keeper);
      }
      if (UpgradeContext.NodeConfiguration==null) {
        if (domain.Configuration.UpgradeMode==DomainUpgradeMode.Skip)
          keeper.AddFinalResult(UpgradeContext.ExtractedSqlModelCache);
        else
          keeper.AddInitialResult(UpgradeContext.ExtractedSqlModelCache);
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Skip)
          keeper.AddFinalResult(UpgradeContext.ExtractedSqlModelCache);
        else
          keeper.AddInitialResult(UpgradeContext.ExtractedSqlModelCache);
      }
    }
  }

  public abstract class SkipBuildingTestBase : AutoBuildTest
  {
    private bool isInitialBuildingFinished;

    private readonly List<SchemaExtractionKeeper> extractionResultKeepers = new List<SchemaExtractionKeeper>();

    internal ReadOnlyCollection<SchemaExtractionKeeper> ExtractionResultKeepers
    {
      get { return extractionResultKeepers.AsReadOnly(); }
    }

    protected virtual void PopulateData(Domain domain)
    {
    }

    protected virtual IEnumerable<NodeConfiguration> BuildNodeConfigurations()
    {
      return Enumerable.Empty<NodeConfiguration>();
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      var initialConfiguration = configuration;
      var finalConfiguration = initialConfiguration.Clone();

      initialConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      finalConfiguration.UpgradeMode = DomainUpgradeMode.Skip;

      using (var domain = base.BuildDomain(initialConfiguration)) {
        BuildNodes(domain);
        PopulateData(domain);
        extractionResultKeepers.Add(domain.Extensions.Get<SchemaExtractionKeeper>());
      }
      var finalDomain = base.BuildDomain(finalConfiguration);
      BuildNodes(finalDomain);
      extractionResultKeepers.Add(finalDomain.Extensions.Get<SchemaExtractionKeeper>());
      return finalDomain;
    }

    protected void BuildNodes(Domain domain, params NodeConfiguration[] nodeConfigurations)
    {
      try {
        foreach (var nodeConfiguration in BuildNodeConfigurations()) {
          nodeConfiguration.UpgradeMode = domain.Configuration.UpgradeMode;
          domain.StorageNodeManager.AddNode(nodeConfiguration);
        }
      }
      catch (Exception e) {
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }
  }
}