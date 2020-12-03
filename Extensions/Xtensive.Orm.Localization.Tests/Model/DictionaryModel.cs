using System;
using System.Globalization;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Localization.Tests.Model
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class AbstractDictionary : Entity
  {
    [Key, Field]
    public int Id { get; set; }

    [Field(Nullable = false, Length = 64)]
    public string Identifier { get; set; }

    // abstract property
    public abstract string Name { get; set; }

    public AbstractDictionary(Session session) : base(session) { }
  }

  public abstract class AbstractLocalizableDictionary<T, TT> : AbstractDictionary, ILocalizable<TT>
      where T : AbstractLocalizableDictionary<T, TT>
      where TT : AbstractDictionaryLocalization<T, TT>
  {
    public override string Name { get => Localizations.Current.Name; set => Localizations.Current.Name = value; }

    [Field]
    public LocalizationSet<TT> Localizations { get; private set; }

    public AbstractLocalizableDictionary(Session session) : base(session) { }
  }

  public abstract class AbstractDictionaryLocalization<T, TT> : Localization<T>
    where TT : AbstractDictionaryLocalization<T, TT>
    where T : AbstractLocalizableDictionary<T, TT>
  {
    [Field(Nullable = false, Length = 512)]
    public string Name { get; set; }

    protected AbstractDictionaryLocalization(Session session, CultureInfo culture, T target) : base(session, culture, target) { }
  }

  public class Country : AbstractLocalizableDictionary<Country, CountryLocalization>, ILocalizable<CountryLocalization>
  {
    public Country(Session session) : base(session) { }
  }

  [HierarchyRoot]
  public class CountryLocalization : AbstractDictionaryLocalization<Country, CountryLocalization>
  {
    public CountryLocalization(Session session, CultureInfo culture, Country target) : base(session, culture, target) { }
  }

}
