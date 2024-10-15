Xtensive.Orm.Reprocessing
=========================

Summary
-------
The extension provides API for reprocessible operations. The reprocessible operation 
should represent a separate block of logic, usually a delegate of a method and be transactional.

Prerequisites
-------------
DataObjects.Net 7.1.x (http://dataobjects.net)

Examples of usage
-----------------
**Examples #1**. Simple reprocessible operation looks like this:

```csharp
  Domain.Execute(session => {
    // Task logic
  });
```

**Expample #2**. Usage of built-in reprocessing strategies.

There are 3 strategies that can be used for task execution:
- **HandleReprocessibleException** strategy
    The strategy catches all reprocessible expections (deadlock and transaction serialization exceptions)
    and makes another attempt to execute the task
- **HandleUniqueConstraintViolation** strategy
    The same as previous one but also catches unique constraint violation exception 
- **NoReprocess** strategy
    No reprocessing is provided

To indicate that a particular strategy should be used, use the following syntax:

```csharp
  Domain.WithStrategy(new HandleReprocessExceptionStrategy())
    .Execute(session => {
      // Task logic
    });
```


Examples of how to configure extension
--------------------------------------

Following examples show different ways to configure extension in configuration files of various types.

**Example #1** Confugure in App.config/Web.config

```xml
<configuration>
  <configSections>
    <!-- ... -->
    <section name="Xtensive.Orm.Reprocessing" 
      type="Xtensive.Orm.Reprocessing.Configuration.ConfigurationSection, Xtensive.Orm.Reprocessing" />
  </configSections>

  <Xtensive.Orm.Reprocessing 
    defaultTransactionOpenMode="New" 
    defaultExecuteStrategy="Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing">
  </Xtensive.Orm.Reprocessing>
</configuration>
```

Such configuration is usually read with ```System.Configuration.ConfigurationManager```.
If project still supports such configurations then Reprocessing configuration will be read automatically when it needs to be read.
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
(```DomainConfiguration```, ```ReprocessingConfiguration```, etc.) have ```Load()``` methods that can recieve this instance.
```ReprocessingConfiguration``` can be read like so

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);
  var reprocessingConfig = ReprocessingConfiguration.Load(configuration);

  // loaded configuration should be manually placed to
  domainConfiguration.ExtensionConfigurations.Set(reprocessingConfig);
```

The ```domainConfiguration.ExtensionConfigurations``` is a new unified place from which the extension will try to get its configuration
instead of calling default parameterless ```Load()``` method, which has not a lot of sense now, though the method is kept as a second source
for backwards compatibility.

For more convenience, ```DomainConfiguration``` extensions are provided, which make code more neater.
For instance,

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);

  // the extension hides getting configuration with ReprocessingConfiguration.Load(configuration)
  // and also putting it to ExtensionConfigurations collection.
  domainConfiguration.ConfigureReprocessingExtension(configuration);
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

After that, as in previous example, the instance can be passed to ```Load``` method of ```ReprocessingConfiguration``` to read extension configuration
and later put it to ```DomainConfiguration.ExtensionConfigurations```

```csharp
  var reprocessingConfiguration = ReprocessingConfiguration.Load(configuration);

  domainConfiguration.ExtensionConfigurations.Set(reprocessingConfiguration);
```

Extension usage will look like

```csharp
  domainConfiguration.ConfigureReprocessingExtension(configuration);
```


**Example #4** Configure using Microsoft.Extensions.Configuration API.

This API allows to have configurations in various forms including JSON and XML formats.
Loading of such files may differ depending on .NET version, check Microsoft manuals for instructions.

Allowed JSON and XML configuration definition look like below

```xml
<configuration>
  <Xtensive.Orm.Reprocessing>
    <DefaultTransactionOpenMode>New</DefaultTransactionOpenMode>
    <DefaultExecuteStrategy>Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing</DefaultExecuteStrategy>
  </Xtensive.Orm.Reprocessing>
</configuration>
```

```json
{
  "Xtensive.Orm.Reprocessing": {
    "DefaultTransactionOpenMode" : "New",
    "DefaultExecuteStrategy" : "Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing"
  }
}
```

The API has certain issues with Xml elements with attributes so it is recommended to use
more up-to-date attributeless nodes.
For JSON it is pretty clear,  almost averyone knows its format.

```ReprocessingConfiguration.Load``` method can accept different types of abstractions from the API, including 
- ```Microsoft.Extensions.Configuration.IConfiguration```;
- ```Microsoft.Extensions.Configuration.IConfigurationRoot```;
- ```Microsoft.Extensions.Configuration.IConfigurationSection```.

Loading of configuration may look like

```csharp
  
  var app = builder.Build();

  //...

  // tries to load from default section "Xtensive.Orm.Reprocessing"
  var reprocessingConfig = ReprocessingConfiguration.Load(app.Configuration);

  domainConfiguration.ExtensionConfigurations.Set(reprocessingConfig);
```

or, with use of extension, like


```csharp
  
  var app = builder.Build();

  //...

  // tries to load from default section "Xtensive.Orm.Reprocessing"
  // and put it into domainConfiguration.ExtensionConfigurations

  domainConfiguration.ConfigureReprocessingExtension(app.Configuration);
```



**Example #5** Configure using Microsoft.Extensions.Configuration API from section with non-default name.

For configurations like

```xml
<configuration>
  <Orm.Reprocessing>
    <DefaultTransactionOpenMode>New</DefaultTransactionOpenMode>
    <DefaultExecuteStrategy>Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing</DefaultExecuteStrategy>
  </Orm.Reprocessing>
</configuration>
```

```json
{
  "Orm.Reprocessing": {
    "DefaultTransactionOpenMode" : "New",
    "DefaultExecuteStrategy" : "Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing"
  }
}
```

Loading of configuration may look like

```csharp
  
  var app = builder.Build();

  //...

  var reprocessingConfig = ReprocessingConfiguration.Load(app.Configuration, "Orm.Reprocessing");

  domainConfiguration.ExtensionConfigurations.Set(reprocessingConfig);
```

or, with use of extension, like

```csharp
  
  var app = builder.Build();

  //...

  domainConfiguration.ConfigureReprocessingExtension(app.Configuration, "Orm.Reprocessing");
```


**Example #6** Configure using Microsoft.Extensions.Configuration API from sub-section deeper in section tree.

If for some reason extension configuration should be moved deeper in section tree like something below

```xml
<configuration>
  <Orm.Extensions>
    <Xtensive.Orm.Reprocessing>
      <DefaultTransactionOpenMode>New</DefaultTransactionOpenMode>
      <DefaultExecuteStrategy>Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing</DefaultExecuteStrategy>
    </Xtensive.Orm.Reprocessing>
  </Orm.Extensions>
</configuration>
```

or in JSON

```json
{
  "Orm.Extensions": {
    "Xtensive.Orm.Reprocessing": {
      "DefaultTransactionOpenMode" : "New",
      "DefaultExecuteStrategy" : "Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing"
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
  var reprocessingSection = extensionsGroupSection.GetSection("Xtensive.Orm.Reprocessing");

  var reprocessingConfig = ReprocessingConfiguration.Load(reprocessingSection);

  domainConfiguration.ExtensionConfigurations.Set(reprocessingConfig);
```

or, with use of extension method, like

```csharp
  
  var app = builder.Build();

  //...

  var configurationRoot = app.Configuration;
  var extensionsGroupSection = configurationRoot.GetSection("Orm.Extensions");
  var reprocessingSection = extensionsGroupSection.GetSection("Xtensive.Orm.Reprocessing");

  domainConfiguration.ConfigureReprocessingExtension(reprocessingSection);
```
