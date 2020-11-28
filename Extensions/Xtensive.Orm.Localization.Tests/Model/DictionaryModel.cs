using System;
using System.Globalization;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Localization.Tests.Model
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class IdentifiedEntity : Entity
  {
    [Key, Field]
    public int Id { get; set; }

    [Version, Field(Nullable = false)]
    public int RecordVersion { get; set; }

    public IdentifiedEntity(Session session) : base(session) { }
  }

  public abstract class AbstractDictionary : IdentifiedEntity
  {
    public const string TableNamePrefix = "Dict_";

    [Field(Nullable = false, Length = 64)]
    public string Identifier { get; set; }
    [Field(Nullable = false)]
    public bool Enabled { get; set; }
    [Field(Nullable = false)]
    public bool Selectable { get; set; }
    [Field(Nullable = false)]
    public bool Displayable { get; set; }
    public abstract string Name { get; set; }
    public abstract string Description { get; set; }

    public AbstractDictionary(Session session) : base(session) { }
  }

  public class AbstractNonLocalizableDictionary : AbstractDictionary
  {
    [Field(Nullable = false, Length = 512)]
    public override string Name { get; set; }
    [Field(Length = 2048, LazyLoad = true)]
    public override string Description { get; set; }

    public AbstractNonLocalizableDictionary(Session session) : base(session) { }
  }

  public abstract class AbstractLocalizableDictionary<T, TT> : AbstractDictionary, ILocalizable<TT>
      where T : AbstractLocalizableDictionary<T, TT>
      where TT : AbstractDictionaryLocalization<T, TT>
  {
    public override string Name { get => Localizations.Current.Name; set => Localizations.Current.Name = value; }
    public override string Description { get => Localizations.Current.Description; set => Localizations.Current.Description = value; }

    [Field]
    public LocalizationSet<TT> Localizations { get; private set; }

    public AbstractLocalizableDictionary(Session session) : base(session) { }
  }

  public abstract class AbstractDictionaryLocalization<T, TT> : Localization<T>
    where TT : AbstractDictionaryLocalization<T, TT>
    where T : AbstractLocalizableDictionary<T, TT>
  {
    public const string TableNamePrefix = AbstractDictionary.TableNamePrefix;
    public const string TableNameSuffix = "_Localization";

    [Field(Nullable = false, Length = 512)]
    public string Name { get; set; }
    [Field(Length = 2048, LazyLoad = true)]
    public string Description { get; set; }

    [Version, Field(Nullable = false)]
    public int RecordVersion { get; set; }

    protected AbstractDictionaryLocalization(Session session, CultureInfo culture, T target) : base(session, culture, target) { }
  }

  [TableMapping(TableNamePrefix + nameof(CommunicationPlatform))]
  public class CommunicationPlatform : AbstractNonLocalizableDictionary
  {
    [Field(Length = 50)]
    public string ProtocolPrefix { get; set; }

    public CommunicationPlatform(Session session) : base(session) { }
  }

  [TableMapping(TableNamePrefix + nameof(BuiltinMessage))]
  public class BuiltinMessage : AbstractLocalizableDictionary<BuiltinMessage, BuiltinMessageLocalization>, ILocalizable<BuiltinMessageLocalization>
  {
    public BuiltinMessage(Session session) : base(session) { }
  }

  [HierarchyRoot]
  [TableMapping(TableNamePrefix + nameof(BuiltinMessage) + TableNameSuffix)]
  public class BuiltinMessageLocalization : AbstractDictionaryLocalization<BuiltinMessage, BuiltinMessageLocalization>
  {
    public BuiltinMessageLocalization(Session session, CultureInfo culture, BuiltinMessage target) : base(session, culture, target) { }
  }

  public class Country : IdentifiedEntity, ILocalizable<CountryLocalization>
  {
    [Field]
    public int OrderValue { get; set; }
    [Field]
    public bool Enabled { get; set; }

    // Localizable field. Note that it is non-persistent
    public string Name
    {
      get => Localizations.Current.Name;
      set => Localizations.Current.Name = value;
    }

    [Field]
    public LocalizationSet<CountryLocalization> Localizations { get; private set; }

    public Country(Session session) : base(session) { }
  }

  [HierarchyRoot]
  public class CountryLocalization : Localization<Country>
  {
    [Field(Length = 100)]
    public string Name { get; set; }
    public CountryLocalization(Session session, CultureInfo culture, Country target) : base(session, culture, target) { }
  }

  [HierarchyRoot]
  public class Color : Entity, ILocalizable<ColorLocalization>
  {
    [Key, Field]
    public int Id { get; set; }

    [Field]
    public int OrderValue { get; set; }
    [Field]
    public bool Enabled { get; set; }

    // Localizable field. Note that it is non-persistent
    public string Name
    {
      get => Localizations.Current.Name;
      set => Localizations.Current.Name = value;
    }

    [Field]
    public LocalizationSet<ColorLocalization> Localizations { get; private set; }

    public Color(Session session) : base(session) { }
  }

  [HierarchyRoot]
  public class ColorLocalization : Localization<Color>
  {
    [Field(Length = 100)]
    public string Name { get; set; }

    public ColorLocalization(Session session, CultureInfo culture, Color target) : base(session, culture, target) { }
  }
}
