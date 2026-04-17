using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelCarga.Models.Users;

public class UserFiltersViewModel
{
    [Display(Name = "Username")]
    public string? Username { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Role")]
    public byte? RoleId { get; set; }

    [Display(Name = "Status")]
    public byte? StatusId { get; set; }

    [Display(Name = "Linked Customer")]
    public bool? HasCustomer { get; set; }
}

public class UserListItemViewModel
{
    public uint Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public byte? RoleId { get; init; }
    public byte? StatusId { get; init; }
    public string RoleName { get; init; } = "Unassigned";
    public string StatusName { get; init; } = "Unassigned";
    public uint? CustomerId { get; init; }
}

public class UserIndexStatsViewModel
{
    public int TotalUsers { get; init; }
    public int MatchingUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int LinkedCustomers { get; init; }
}

public class UserIndexViewModel
{
    public List<UserListItemViewModel> Users { get; init; } = [];
    public UserFiltersViewModel Filters { get; init; } = new();
    public List<SelectListItem> RoleOptions { get; init; } = [];
    public List<SelectListItem> StatusOptions { get; init; } = [];
    public UserIndexStatsViewModel Stats { get; init; } = new();
    public string ActiveFilterSummary { get; init; } = "Showing the full user directory.";
}

public class UserFormViewModel
{
    public uint Id { get; set; }

    [Required]
    [Display(Name = "Username")]
    [StringLength(60, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Credential Hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public byte RoleId { get; set; }

    [Required]
    [Display(Name = "Status")]
    public byte StatusId { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = [];
    public List<SelectListItem> StatusOptions { get; set; } = [];
    public string PageTitle { get; set; } = string.Empty;
    public string IntroText { get; set; } = string.Empty;
    public string SubmitLabel { get; set; } = string.Empty;
    public string HeroEyebrow { get; set; } = string.Empty;
}

public class UserDetailsViewModel
{
    public UserListItemViewModel User { get; init; } = new();
    public string RoleDescription { get; init; } = "No role description available.";
    public string StatusDescription { get; init; } = "No status description available.";
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool HasStoredCredential { get; init; }
    public string CredentialPreview { get; init; } = "Not available";
}

public class UserLookupUserViewModel
{
    public uint Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public byte RoleId { get; init; }
    public string RoleName { get; init; } = "Unassigned";
    public byte StatusId { get; init; }
    public uint? CustomerId { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool HasStoredCredential { get; init; }
}

public class UserLookupResultViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string QueryLabel { get; init; } = string.Empty;
    public string QueryValue { get; init; } = string.Empty;
    public string ResultLabel { get; init; } = string.Empty;
    public string? ScalarResult { get; init; }
    public bool? BooleanResult { get; init; }
    public UserLookupUserViewModel? UserResult { get; init; }
    public string? NotFoundMessage { get; init; }
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterCustomerUserViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(255, MinimumLength = 6)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation must match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Document number")]
    [StringLength(40)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First name")]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    [StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Phone")]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Address")]
    [StringLength(180)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "City")]
    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Country")]
    [StringLength(80)]
    public string Country { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}