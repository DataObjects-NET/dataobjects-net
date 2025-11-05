Xtensive.Orm.Localization
=========================

Summary
-------
The extension transparently solves a task of application or service localization.
This implies that localizable resources are a part of domain model so they are stored in database.

Prerequisites
-------------
DataObjects.Net 7.1.x or later (http://dataobjects.net)

Implementation
--------------

Implement ILocalizable<TLocalization> on your localizable entities, e.g.:

```csharp
  [HierarchyRoot]
  public class Page : Entity, ILocalizable<PageLocalization>
  {
    [Field, Key]
    public int Id { get; private set; }

    // Localizable field. Note that it is non-persistent
    public string Title
    {
      get { return Localizations.Current.Title; }
      set { Localizations.Current.Title = value; }
    }

    [Field] // This is a storage of all localizations for Page class
    public LocalizationSet<PageLocalization> Localizations { get; private set; }

    public Page(Session session) : base(session) {}
  }
```

Define corresponding localizations, e.g.:

```csharp
  [HierarchyRoot]
  public class PageLocalization : Localization<Page>
  {
    [Field(Length = 100)]
    public string Title { get; set; }

    public PageLocalization(Session session, CultureInfo culture, Page target)
      : base(session, culture, target) {}
  }
```


Examples of usage
-----------------

**Example #1**. Access localizable properties as regular ones, e.g.:

```csharp
  page.Title = "Welcome";
  string title = page.Title;
```

**Example #2**. Mass editing of localizable properties:

```csharp
  var en = new CultureInfo("en-US");
  var sp = new CultureInfo("es-ES");
  var page = new Page(session);
  page.Localizations[en].Title = "Welcome";
  page.Localizations[sp].Title = "Bienvenido";
```

**Example #3**. Value of localizable properties reflects culture of the current Thread, e.g.:

```csharp
  Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
  string title = page.Title; // title is "Welcome"

  Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
  string title = page.Title; // title is "Bienvenido"
```

**Example #4**. Instead of altering CurrentThread, instance of LocalizationScope can be used, e.g.:

```csharp
  using (new LocalizationScope(new CultureInfo("en-US"))) {
    string title = page.Title; // title is "Welcome"
  }

  using (new LocalizationScope(new CultureInfo("es-ES"))) {
    string title = page.Title; // title is "Bienvenido"
  }
```

**Example #5**. LINQ queries that include localizable properties are transparently translated

```csharp
  Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
  var query = from p in session.Query.All<Page>()
    where p.Title=="Welcome"
    select p;
  Assert.AreEqual(1, query.Count());

  Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
  var query = from p in session.Query.All<Page>()
    where p.Title=="Bienvenido"
    select p;
  Assert.AreEqual(1, query.Count());
```


Examples of how to configure extension
--------------------------------------

Following examples show different ways to configure extension in configuration files of various types.

**Example #1** Confugure default culture in App.config/Web.config

```xml
<configuration>
  <configSections>
    <section name="Xtensive.Orm" type="Xtensive.Orm.Configuration.Elements.ConfigurationSection, Xtensive.Orm"/>
    <section name="Xtensive.Orm.Localization" type="Xtensive.Orm.Localization.Configuration.ConfigurationSection, Xtensive.Orm.Localization"/>
  </configSections>
  <Xtensive.Orm>
    <!-- domain(s) configured -->
  </Xtensive.Orm>
  <Xtensive.Orm.Localization>
    <defaultCulture name="es-ES"/>
  </Xtensive.Orm.Localization>
</configuration>
```

Such configuration is usually read with ```System.Configuration.ConfigurationManager```.
If project still supports such configurations then Localization configuration will be read automatically when it needs to be read.
Sometimes a work-around is needed to read such configuration, for more read Example #2 and Example #3


**Example #2** Reading old-style configuration of an assembly in NET 5 and newer.

Due to new architecture without AppDomain (which among the other things was in charge of gathering configuration files of loaded assemblies
as it would be one configuration file) ```System.Configuration.ConfigurationManager``` now reads only configuration file of actual executable, loaded 
assemblies' configuration files stay unreachable by default, though there is need to read some data from them.
A great example is test projects which are usually get loaded by test runner executable, and the only configuration accessible in this case
is test runner one.

Extra step is required to read configuration files in such cases. Thankfully, ```ConfigurationManager``` has methods to get access to assemblies' configuration files.

To get access to an assembly configuration file it should be opened explicitly by

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);
```

The instance returned from ```OpenExeConfiguration``` provides access to sections of the assembly configuration. DataObjects.Net configurations
(```DomainConfiguration```, ```LocalizationConfiguration```, etc.) have ```Load()``` methods that can recieve this instance.
```LocalizationConfiguration``` can be read like so

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);
  var localizationConfig = LocalizationConfiguration.Load(configuration);

  // loaded configuration should be manually placed to
  domainConfiguration.ExtensionConfigurations.Set(localizationConfig);
```

The ```domainConfiguration.ExtensionConfigurations``` is a new unified place from which the extension will try to get its configuration
instead of calling default parameterless ```Load()``` method, which has not a lot of sense now, though the method is kept as a second source
for backwards compatibility.

For more convenience, ```DomainConfiguration``` extensions are provided, which make code neater.
For instance,

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);

  // the extension hides getting configuration with LocalizationConfiguration.Load(configuration)
  // and also putting it to ExtensionConfigurations collection.
  domainConfiguration.ConfigureLocalizationExtension(configuration);
```

Custom section names are also supported if for some reason default section name is not used.


**Example #3** Reading old-style configuration of an assembly in a project that uses appsettings.json file.

If for some reason there is need to keep the old-style configuration then there is a work-around as well.
Static configuration manager provides method ```OpenMappedExeConfiguration()``` which allows to get 
any *.config file as ```System.Configuration.Configuration``` instance. For example,

```csharp
  ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
  configFileMap.ExeConfigFilename = "Orm.config"; //or other file name, the file should exist bin folder
  var configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
```

After that, as in previous example, the instance can be passed to ```Load``` method of ```LocalizationConfiguration``` to read extension configuration
and later put it to ```DomainConfiguration.ExtensionConfigurations```

```csharp
  var localizationConfiguration = LocalizationConfiguration.Load(configuration);

  domainConfiguration.ExtensionConfigurations.Set(localizationConfiguration);
```

Extension usage will look like

```csharp
  domainConfiguration.ConfigureLocalizationExtension(configuration);
```


**Example #4** Configure using Microsoft.Extensions.Configuration API.

This API allows to have configurations in various forms including JSON and XML formats.
Loading of such files may differ depending on .NET version, check Microsoft manuals for instructions.

Allowed JSON and XML configuration definition look like below

```xml
<configuration>
  <Xtensive.Orm.Localization>
    <DefaultCulture>es-ES</DefaultCulture>
  </Xtensive.Orm.Localization>
</configuration>
```

```json
{
  "Xtensive.Orm.Localization": {
    "DefaultCulture": "es-ES"
  }
}
```

The API has certain issues with XML elements with attributes so it is recommended to use
more up-to-date attributeless nodes.
For JSON it is pretty clear, almost averyone knows its format.

```LocalizationConfiguration.Load``` method can accept different types of abstractions from the API, including
- ```Microsoft.Extensions.Configuration.IConfiguration```;
- ```Microsoft.Extensions.Configuration.IConfigurationRoot```;
- ```Microsoft.Extensions.Configuration.IConfigurationSection```.

Loading of configuration may look like

```csharp
  
  var app = builder.Build();

  //...

  // tries to load from default section "Xtensive.Orm.Localization"
  var localizationConfig = LocalizationConfiguration.Load(app.Configuration);

  domainConfiguration.ExtensionConfigurations.Set(localizationConfig);
```

or, with use of extension, like


```csharp
  
  var app = builder.Build();

  //...

  // tries to load from default section "Xtensive.Orm.Localization"
  // and additionally adds Xtensive.Orm.Localization assembly to domain types.

  domainConfiguration.ConfigureLocalizationExtension(app.Configuration);
```



**Example #5** Configure using Microsoft.Extensions.Configuration API from section with non-default name.

For configurations like

```xml
  <configuration>
    <Orm.Localization>
      <DefaultCulture>es-ES</DefaultCulture>
    </Orm.Localization>
  </configuration>
```

```json
{
  "Orm.Localization": {
    "DefaultCulture": "es-ES"
  }
}
```

Loading of configuration may look like

```csharp
  
  var app = builder.Build();

  //...

  var localizationConfig = LocalizationConfiguration.Load(app.Configuration, "Orm.Localization");

  domainConfiguration.ExtensionConfigurations.Set(localizationConfig);
```

or, with use of extension, like

```csharp
  var app = builder.Build();
  domainConfiguration.ConfigureLocalizationExtension(app.Configuration, "Orm.Localization");
```


**Example #6** Configure using Microsoft.Extensions.Configuration API from sub-section deeper in section tree.

If for some reason extension configuration should be moved deeper in section tree like something below

```xml
<configuration>
  <Orm.Extensions>
    <Xtensive.Orm.Localization>
      <DefaultCulture>es-ES</DefaultCulture>
    </Xtensive.Orm.Localization>
  </Orm.Extensions>
</configuration>
```

or in JSON

```json
{
  "Orm.Extensions": {
    "Xtensive.Orm.Localization": {
      "DefaultCulture": "es-ES"
    }
  }
}
```

Then section must be provided manually, code may look like

```csharp
  
  var app = builder.Build();

  //...

  var configurationRoot = app.Configuration;
  var extensionsGroupSection = configurationRoot.GetSection("Orm.Extensions");
  var localizationSection = extensionsGroupSection.GetSection("Xtensive.Orm.Localization");
  var localizationConfig = LocalizationConfiguration.Load(localizationSection);

  domainConfiguration.ExtensionConfigurations.Set(localizationConfig);
```

or, with use of extension method, like

```csharp
  
  var app = builder.Build();

  //...

  var configurationRoot = app.Configuration;
  var extensionsGroupSection = configurationRoot.GetSection("Orm.Extensions");
  var localizationSection = extensionsGroupSection.GetSection("Xtensive.Orm.Localization");

  domainConfiguration.ConfigureLocalizationExtension(localizationSection);
```