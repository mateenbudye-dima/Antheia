namespace Antheia.Domain.CondorEntities;

public partial class ContactInfo
{
    public int ContactId { get; set; }

    public string? Title { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? Avatar { get; set; }

    public short? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public byte? MaritalStatusId { get; set; }

    public string? EmailAddress { get; set; }

    public string? HomePhone { get; set; }

    public string? MobilePhone { get; set; }

    public string? WorkPhone { get; set; }

    public string? Fax { get; set; }

    public string? Suffix { get; set; }

    public string? WhatsappNumber { get; set; }

    public string? SkypeId { get; set; }

    public string? WeChatId { get; set; }

    public string? Qualification { get; set; }

    public int? MonthsOfExperience { get; set; }

    public DateTime? ExperienceSpecifiedOn { get; set; }

    /// <summary>
    /// 0=N 1=Y
    /// </summary>
    public bool IsActive { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<UserContact> UserContacts { get; set; } = new List<UserContact>();
}
