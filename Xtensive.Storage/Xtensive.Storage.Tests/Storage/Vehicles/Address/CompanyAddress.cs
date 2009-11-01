using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Company address. Extends <see cref="Address"/> with company-specific fields.
  /// </summary>
  public class CompanyAddress : Address
  {
    [Field]
    public string MailAddress { get; set; }

    [Field(Length = 100)]
    public string Fax { get; set; }

    [Field(IsNullable = true)]
    public virtual string Phone { get; set; }

    [Field(IsNullable = true)]
    public string Email { get; set; }
  }
}