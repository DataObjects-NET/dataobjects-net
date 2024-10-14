// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration.Options;

namespace Xtensive.Orm.Configuration
{
  internal sealed class XmlToDomainConfigurationReader : DomainConfigurationReader
  {
    protected override bool ValidateCorrectFormat(IConfigurationSection allDomainsSection, string domainName)
    {
      return !allDomainsSection.GetSection(domainName).GetChildren().Any()
        && allDomainsSection.GetSection(string.Format(NamedDomainTemplate, domainName)).GetChildren().Any();
    }

    protected override IConfigurationSection GetDomainSection(IConfigurationSection allDomainsSection, string domainName) =>
      allDomainsSection.GetSection(string.Format(NamedDomainTemplate, domainName));

    protected override void ProcessNamingConvention(IConfigurationSection namingConventionSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
      if (namingConventionSection == null) {
        domainConfiguratonOptions.NamingConventionRaw = null;
        return;
      }

      if (namingConventionSection.GetChildren().Any()) {
        var namingConvetion = namingConventionSection.Get<NamingConventionOptions>();
        if (namingConvetion!= null) {
          var synonymsSection = namingConventionSection.GetSection(NamespaceSynonymSectionName);
          var synonyms = synonymsSection != null && synonymsSection.GetChildren().Any()
            ? synonymsSection.GetSection(SynonymElementName)
              .GetSelfOrChildren()
              .Select(s => s.Get<NamespaceSynonymOptions>())
              .Where(ns => ns != null)
              .ToArray()
            : Array.Empty<NamespaceSynonymOptions>();

          namingConvetion.NamespaceSynonyms = synonyms;
          domainConfiguratonOptions.NamingConventionRaw = namingConvetion;
        }
        else {
          domainConfiguratonOptions.NamingConventionRaw = null;
        }
      }
    }

    protected override void ProcessVersioningConvention(IConfigurationSection versioningConventionSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
      if (versioningConventionSection == null) {
        domainConfiguratonOptions.VersioningConvention = null;
        return;
      }

      if (versioningConventionSection.GetChildren().Any()) {
        var versioningConvention = versioningConventionSection.Get<VersioningConventionOptions>();

        domainConfiguratonOptions.VersioningConvention = versioningConvention != null
          ? versioningConvention
          : null;
      }
    }

    protected override void ProcessTypes(IConfigurationSection typesSection, DomainConfigurationOptions domainConfigurationOptions)
    {
      if (typesSection == null) {
        domainConfigurationOptions.Types = Array.Empty<TypeRegistrationOptions>();
        return;
      }
      if (TryProcessTypeRegistrationsWithAttributes(typesSection, domainConfigurationOptions)
        || TryProcessTypeRegistrationsWithNodes(typesSection, domainConfigurationOptions)) {
        return;
      }

      domainConfigurationOptions.Types = Array.Empty<TypeRegistrationOptions>();
    }

    private bool TryProcessTypeRegistrationsWithAttributes(IConfigurationSection typesSection,
      DomainConfigurationOptions domainConfigurationOptions)
    {
      var registrations = typesSection.GetSection(OldStyleTypeRegistrationElementName);
      if (registrations != null && registrations.GetChildren().Any()) {
        domainConfigurationOptions.Types = registrations
          .GetSelfOrChildren()
          .Select(s => s.Get<TypeRegistrationOptions>())
          .Where(tr => tr != null)
          .ToArray();
        return true;
      }
      return false;
    }

    private bool TryProcessTypeRegistrationsWithNodes(IConfigurationSection typesSection,
      DomainConfigurationOptions domainConfigurationOptions)
    {
      var registrations = typesSection.GetSection(TypeRegistrationElementName);
      if (registrations == null)
        return false;

      domainConfigurationOptions.Types = registrations
        .GetSelfOrChildren()
        .Select(s => s.Get<TypeRegistrationOptions>())
        .Where(tr => tr != null)
        .ToArray();
      return true;
    }

    protected override void ProcessDatabases(IConfigurationSection databasesSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
      domainConfiguratonOptions.Databases.Clear();
      if (databasesSection == null)
        return;
      var databaseElement = databasesSection.GetSection(DatabaseElementName);
      if (databaseElement == null)
        return;
      foreach (var section in databaseElement.GetSelfOrChildren(true)) {
        var dbItem = section.Get<DatabaseOptions>();
        if (dbItem == null)
          continue;
        domainConfiguratonOptions.Databases.Add(dbItem.Name, dbItem);
      }
    }

    protected override void ProcessKeyGenerators(IConfigurationSection keyGeneratorsSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
      domainConfiguratonOptions.KeyGenerators.Clear();
      ProcessCollectionOfOptions(keyGeneratorsSection, KeyGeneratorElementName, domainConfiguratonOptions.KeyGenerators);
    }

    protected override void ProcessIgnoreRules(IConfigurationSection ignoreRulesSection, Options.DomainConfigurationOptions domainConfiguratonOptions)
    {
      domainConfiguratonOptions.IgnoreRules.Clear();
      ProcessCollectionOfOptions(ignoreRulesSection, RuleElementName, domainConfiguratonOptions.IgnoreRules);
    }

    protected override void ProcessMappingRules(IConfigurationSection mappingRulesSection, Options.DomainConfigurationOptions domainConfiguratonOptions)
    {
      domainConfiguratonOptions.MappingRules.Clear();
      ProcessCollectionOfOptions(mappingRulesSection, RuleElementName, domainConfiguratonOptions.MappingRules);
    }

    protected override void ProcessSessions(IConfigurationSection sessionsSection, Options.DomainConfigurationOptions domainConfiguratonOptions)
    {
      domainConfiguratonOptions.Sessions.Clear();
      if (sessionsSection == null)
        return;
      var sessionElement = sessionsSection.GetSection(SessionElementName);
      if (sessionElement == null)
        return;
      foreach (var section in sessionElement.GetSelfOrChildren(true)) {
        var sessionItem = section.Get<SessionConfigurationOptions>();
        if (sessionItem == null)
          continue;
        domainConfiguratonOptions.Sessions.Add(sessionItem.Name, sessionItem);
      }
    }

    private void ProcessCollectionOfOptions<TOption>(IConfigurationSection collectionSection, string itemKey, OptionsCollection<TOption> collection)
      where TOption : class, IIdentifyableOptions
    {
      if (collectionSection == null)
        return;
      var collectionElement = collectionSection.GetSection(itemKey);
      if (collectionElement == null)
        return;
      foreach (var item in collectionElement.GetSelfOrChildren()) {
        var optItem = item.Get<TOption>();
        collection.Add(optItem);
      }
    }
  }

  internal sealed class JsonToDomainConfigurationReader : DomainConfigurationReader
  {
    protected override bool ValidateCorrectFormat(IConfigurationSection allDomainsSection, string domainName)
    {
      return allDomainsSection.GetSection(domainName).GetChildren().Any()
        && !allDomainsSection.GetSection(string.Format(NamedDomainTemplate, domainName)).GetChildren().Any();
    }

    protected override IConfigurationSection GetDomainSection(IConfigurationSection allDomainsSection, string domainName) =>
      allDomainsSection.GetSection(domainName);

    protected override void ProcessNamingConvention(IConfigurationSection namingConventionSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
      if (namingConventionSection == null || !namingConventionSection.GetChildren().Any())
        return;
      var jsonListVariant = namingConventionSection.Get<NamingConventionOptions>();
      if (jsonListVariant.NamespaceSynonyms == null)
        jsonListVariant.NamespaceSynonyms = Array.Empty<NamespaceSynonymOptions>();
      domainConfiguratonOptions.NamingConventionRaw = jsonListVariant;
    }
  }

  internal abstract class DomainConfigurationReader : IConfigurationSectionReader<DomainConfiguration>
  {
    protected sealed class ConfigurationParserContext
    {
      public readonly IConfigurationRoot CurrentConfiguration;

      public readonly IConfigurationSection CurrentSection;

      public readonly string SectionName;

      public readonly IDictionary<string, string> ConnectionStrings;

      public ConfigurationParserContext(IConfigurationRoot currentConfiguration, string sectionName)
      {
        CurrentConfiguration = currentConfiguration;
        CurrentSection = currentConfiguration.GetSection(sectionName);
        ConnectionStrings = currentConfiguration.Get<Dictionary<string, string>>();
      }

      public ConfigurationParserContext(IConfigurationSection currentSection, IDictionary<string, string> connectionStrings)
      {
        CurrentSection = currentSection;
        ConnectionStrings = connectionStrings;
      }
    }

    protected const string RuleElementName = "Rule";
    protected const string KeyGeneratorElementName = "KeyGenerator";
    protected const string OldStyleTypeRegistrationElementName = "Add";
    protected const string TypeRegistrationElementName = "Registration";
    protected const string SynonymElementName = "Synonym";
    protected const string DatabaseElementName = "Database";
    protected const string SessionElementName = "Session";

    protected const string NamingConventionSectionName = "NamingConvention";
    protected const string VersioningConventionSectionName = "VersioningConvention";
    protected const string TypesSectionName = "Types";
    protected const string DatabasesSectionName = "Databases";
    protected const string KeyGeneratorsSectionName = "KeyGenerators";
    protected const string IgnoreRulesSectionName = "IgnoreRules";
    protected const string MappingRulesSectionName = "MappingRules";
    protected const string SessionsSectionName = "Sessions";
    protected const string NamespaceSynonymSectionName = "NamespaceSynonyms";
    protected const string DomainsSectionName = "Domains";

    protected const string NamedDomainTemplate = "Domain:{0}";

    /// <summary>
    /// Gets domain configuration with name "Default" from <see cref="WellKnown.DefaultConfigurationSection">default section</see>.
    /// </summary>
    /// <param name="configurationRoot">Configration root.</param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Read(IConfigurationRoot configurationRoot) =>
      Read(configurationRoot, WellKnown.DefaultConfigurationSection, WellKnown.DefaultDomainConfigurationName);

    /// <summary>
    /// Gets domain configuration with name "Default" from <paramref name="configurationRoot"/>.
    /// </summary>
    /// <param name="configurationRoot">Configration root</param>
    /// <param name="sectionName">Custom section name where domains are placed.</param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Read(IConfigurationRoot configurationRoot, string sectionName) =>
      Read(configurationRoot, sectionName, WellKnown.DefaultDomainConfigurationName);

    /// <summary>
    /// Gets domain configuration with given name from <paramref name="configurationRoot"/>.
    /// </summary>
    /// <param name="configurationRoot">Configration root.</param>
    /// <param name="sectionName">Custom section name where domains are placed.</param>
    /// <param name="nameOfConfiguration">Name of domain configuration.</param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Read(IConfigurationRoot configurationRoot, string sectionName, string nameOfConfiguration)
    {
      var allDomainsSection = configurationRoot.GetSection(sectionName).GetSection(DomainsSectionName);
      if (allDomainsSection == null || !allDomainsSection.GetChildren().Any())
        return null;

      var domainConfigurationSection = GetDomainSection(allDomainsSection, nameOfConfiguration);
      if (domainConfigurationSection == null || !domainConfigurationSection.GetChildren().Any())
        return null;

      var connectionStrings = configurationRoot.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
      var context = new ConfigurationParserContext(domainConfigurationSection, connectionStrings);

      return ReadInternal(context);
    }

    /// <summary>
    /// Gets domain configuration with name "Default" from <paramref name="rootSection"/>.
    /// </summary>
    /// <param name="rootSection">Root section where all domain configurations stored, by default <see cref="WellKnown.DefaultConfigurationSection"/></param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Read(IConfigurationSection rootSection) =>
      Parse(rootSection, WellKnown.DefaultDomainConfigurationName, null);

    /// <summary>
    /// Gets domain configuration with given name from <paramref name="rootSection"/>.
    /// </summary>
    /// <param name="rootSection">Root section where all domain configurations stored, by default <see cref="WellKnown.DefaultConfigurationSection"/></param>
    /// <param name="nameOfConfiguration">Name of domain configuration.</param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Read(IConfigurationSection rootSection, string nameOfConfiguration) =>
      Parse(rootSection, nameOfConfiguration, null);

    /// <summary>
    /// Gets domain configuration with name "Default" from <paramref name="rootSection"/>.
    /// </summary>
    /// <param name="rootSection">Root section where all domain configurations stored, by default <see cref="WellKnown.DefaultConfigurationSection"/></param>
    /// <param name="connectionStrings">
    /// Connection strings in form of dictionary of strings. Required if connection string alias is used in domain configuration connection settings
    /// </param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Parse(IConfigurationSection rootSection, Dictionary<string, string> connectionStrings)
      => Parse(rootSection, WellKnown.DefaultDomainConfigurationName, connectionStrings);

    /// <summary>
    /// Gets domain configuration with given name from <paramref name="rootSection"/>.
    /// </summary>
    /// <param name="rootSection">Root section where all domain configurations stored, by default <see cref="WellKnown.DefaultConfigurationSection"/></param>
    /// <param name="nameOfConfiguration">Name of domain configuration.</param>
    /// <param name="connectionStrings">
    /// Connection strings in form of dictionary of strings. Required if connection string alias is used in domain configuration connection settings
    /// </param>
    /// <returns>DomainConfiguration instance if it was read successfully, otherwise, <see langword="null"/>.</returns>
    public DomainConfiguration Parse(IConfigurationSection rootSection, string nameOfConfiguration, Dictionary<string, string> connectionStrings)
    {
      var allDomainsSection = rootSection.GetSection(DomainsSectionName);
      if (allDomainsSection == null || !allDomainsSection.GetChildren().Any())
        return null;

      var domainConfigurationSection = GetDomainSection(allDomainsSection, nameOfConfiguration);
      if (domainConfigurationSection == null || !domainConfigurationSection.GetChildren().Any())
        return null;

      var context = new ConfigurationParserContext(domainConfigurationSection, connectionStrings);
      return ReadInternal(context);
    }

    protected abstract bool ValidateCorrectFormat(IConfigurationSection allDomainsSection, string domainName);

    protected abstract IConfigurationSection GetDomainSection(IConfigurationSection allDomainsSection, string domainName);

    private DomainConfiguration ReadInternal(ConfigurationParserContext context)
    {
      var domainByNameSection = context.CurrentSection;
      if (domainByNameSection == null || !domainByNameSection.GetChildren().Any()) {
        return null;
      }
      // this handles only root properties of domain configuration
      var domainConfigurationOptions = context.CurrentSection.Get<Options.DomainConfigurationOptions>();
      if (domainConfigurationOptions == null) {
        return null;
      }

      // all sub-items require manual reading;
      ProcessNamingConvention(domainByNameSection.GetSection(NamingConventionSectionName), domainConfigurationOptions);
      ProcessVersioningConvention(domainByNameSection.GetSection(VersioningConventionSectionName), domainConfigurationOptions);
      ProcessTypes(domainByNameSection.GetSection(TypesSectionName), domainConfigurationOptions);
      ProcessDatabases(domainByNameSection.GetSection(DatabasesSectionName), domainConfigurationOptions);
      ProcessKeyGenerators(domainByNameSection.GetSection(KeyGeneratorsSectionName), domainConfigurationOptions);
      ProcessIgnoreRules(domainByNameSection.GetSection(IgnoreRulesSectionName), domainConfigurationOptions);
      ProcessMappingRules(domainByNameSection.GetSection(MappingRulesSectionName), domainConfigurationOptions);
      ProcessSessions(domainByNameSection.GetSection(SessionsSectionName), domainConfigurationOptions);

      return domainConfigurationOptions.ToNative(context.ConnectionStrings);
    }

    protected virtual void ProcessNamingConvention(IConfigurationSection namingConventionSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }

    protected virtual void ProcessVersioningConvention(IConfigurationSection versioningConventionSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }

    protected virtual void ProcessTypes(IConfigurationSection typesSection, DomainConfigurationOptions domainConfigurationOptions)
    {
    }

    protected virtual void ProcessDatabases(IConfigurationSection databasesSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }

    protected virtual void ProcessKeyGenerators(IConfigurationSection keyGeneratorsSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }

    protected virtual void ProcessIgnoreRules(IConfigurationSection ignoreRulesSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }

    protected virtual void ProcessMappingRules(IConfigurationSection mappingRulesSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }

    protected virtual void ProcessSessions(IConfigurationSection sessionsSection, DomainConfigurationOptions domainConfiguratonOptions)
    {
    }
  }
}