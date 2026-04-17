using HotelCarga.Models.Users;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace HotelCarga.Web.Controllers;

public class UserController : Controller
{
    private const string ApiUnavailableMessage = "User service is unavailable. Start HotelCarga.ApiModel and try again.";

    private readonly IUserApiService _service;
    private readonly ICustomerApiService _customerService;

    public UserController(IUserApiService service, ICustomerApiService customerService)
    {
        _service = service;
        _customerService = customerService;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _service.GetUserWithRoleByEmailAsync(model.Email.Trim());
            if (user is null || !string.Equals(user.password_hash, model.Password, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            if (!string.Equals(user.status?.status_name, "Active", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "This account is not active.");
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);
            return RedirectToLocal(model.ReturnUrl);
        }
        catch (ApiRequestException ex)
        {
            ModelState.AddModelError(string.Empty, BuildApiErrorMessage(ex));
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
            return View(model);
        }
    }

    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new RegisterCustomerUserViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterCustomerUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            if (await _service.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            var documentTask = _customerService.GetByDocumentNumberAsync(model.DocumentNumber.Trim());
            var phoneTask = _customerService.GetByPhoneAsync(model.Phone.Trim());
            await Task.WhenAll(documentTask, phoneTask);

            if (documentTask.Result is not null)
            {
                ModelState.AddModelError(nameof(model.DocumentNumber), "This document number is already associated to an existing customer.");
            }

            if (phoneTask.Result is not null)
            {
                ModelState.AddModelError(nameof(model.Phone), "This phone number is already associated to an existing customer.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var rolesTask = _service.GetRolesAsync();
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(rolesTask, statusesTask);

            var roles = rolesTask.Result;
            var statuses = statusesTask.Result;

            var customerRole = roles.FirstOrDefault(role =>
                string.Equals(role.role_name, "Customer", StringComparison.OrdinalIgnoreCase))
                ?? roles.FirstOrDefault();

            var activeStatus = statuses.FirstOrDefault(status =>
                string.Equals(status.status_name, "Active", StringComparison.OrdinalIgnoreCase))
                ?? statuses.FirstOrDefault();

            if (customerRole is null || activeStatus is null)
            {
                ModelState.AddModelError(string.Empty, "Unable to resolve registration defaults. Validate role and status catalogs.");
                return View(model);
            }

            var normalizedEmail = model.Email.Trim();
            var userId = await _service.CreateAsync(new SaveUserDto(
                normalizedEmail,
                normalizedEmail,
                model.Password,
                customerRole.id,
                activeStatus.id));

            try
            {
                await _customerService.CreateAsync(new SaveCustomerDto(
                    userId,
                    model.DocumentNumber.Trim(),
                    model.FirstName.Trim(),
                    model.LastName.Trim(),
                    model.Phone.Trim(),
                    model.Address.Trim(),
                    model.City.Trim(),
                    model.Country.Trim()));
            }
            catch
            {
                await TryDeleteUserAsync(userId);
                throw;
            }

            var createdUser = await _service.GetEditableByIdAsync(userId);
            if (createdUser is not null)
            {
                await SignInUserAsync(createdUser, rememberMe: true);
            }

            TempData["SuccessMessage"] = "Your account was created successfully.";
            return RedirectToLocal(model.ReturnUrl);
        }
        catch (ApiRequestException ex)
        {
            ModelState.AddModelError(string.Empty, BuildApiErrorMessage(ex));
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Index(UserFiltersViewModel filters)
    {
        List<UserSummaryDto> allUsers;
        List<UserRoleDto> roles;
        List<UserStatusDto> statuses;

        try
        {
            var usersTask = _service.GetAllAsync();
            var rolesTask = _service.GetRolesAsync();
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(usersTask, rolesTask, statusesTask);

            allUsers = usersTask.Result;
            roles = rolesTask.Result;
            statuses = statusesTask.Result;
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return View(BuildUnavailableIndexModel(filters));
        }

        var filteredUsers = allUsers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.Username))
        {
            filteredUsers = filteredUsers.Where(user => user.username.Contains(filters.Username, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Email))
        {
            filteredUsers = filteredUsers.Where(user => user.email.Contains(filters.Email, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.RoleId.HasValue)
        {
            var selectedRoleName = roles.FirstOrDefault(role => role.id == filters.RoleId.Value)?.role_name;
            if (!string.IsNullOrWhiteSpace(selectedRoleName))
            {
                filteredUsers = filteredUsers.Where(user => string.Equals(user.role, selectedRoleName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (filters.StatusId.HasValue)
        {
            var selectedStatusName = statuses.FirstOrDefault(status => status.id == filters.StatusId.Value)?.status_name;
            if (!string.IsNullOrWhiteSpace(selectedStatusName))
            {
                filteredUsers = filteredUsers.Where(user => string.Equals(user.status, selectedStatusName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (filters.HasCustomer.HasValue)
        {
            filteredUsers = filteredUsers.Where(user => user.customer.HasValue == filters.HasCustomer.Value);
        }

        var filteredList = filteredUsers
            .OrderBy(user => user.username, StringComparer.OrdinalIgnoreCase)
            .Select(user => MapSummary(user, roles, statuses))
            .ToList();

        return View(new UserIndexViewModel
        {
            Filters = filters,
            Users = filteredList,
            RoleOptions = BuildRoleOptions(roles, filters.RoleId),
            StatusOptions = BuildStatusOptions(statuses, filters.StatusId),
            ActiveFilterSummary = BuildFilterSummary(filters, roles, statuses),
            Stats = new UserIndexStatsViewModel
            {
                TotalUsers = allUsers.Count,
                MatchingUsers = filteredList.Count,
                ActiveUsers = allUsers.Count(user => string.Equals(user.status, "Active", StringComparison.OrdinalIgnoreCase)),
                LinkedCustomers = allUsers.Count(user => user.customer.HasValue)
            }
        });
    }

    public async Task<IActionResult> Details(uint id)
    {
        try
        {
            var summaryTask = _service.GetByIdAsync(id);
            var editableTask = _service.GetEditableByIdAsync(id);
            var rolesTask = _service.GetRolesAsync();
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(summaryTask, editableTask, rolesTask, statusesTask);

            var summary = summaryTask.Result;
            if (summary is null)
            {
                return NotFound();
            }

            var editable = editableTask.Result;
            var roles = rolesTask.Result;
            var statuses = statusesTask.Result;

            var role = editable is null ? null : roles.FirstOrDefault(item => item.id == editable.role_id);
            var status = editable is null ? null : statuses.FirstOrDefault(item => item.id == editable.status_id);

            return View(new UserDetailsViewModel
            {
                User = MapSummary(summary, roles, statuses, editable?.role_id, editable?.status_id),
                RoleDescription = role?.description ?? "No role description available.",
                StatusDescription = status?.description ?? "No status description available.",
                CreatedAt = editable?.created_at,
                UpdatedAt = editable?.updated_at,
                HasStoredCredential = !string.IsNullOrWhiteSpace(editable?.password_hash),
                CredentialPreview = BuildCredentialPreview(editable?.password_hash)
            });
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult GetAll()
    {
        return RedirectToAction(nameof(Index));
    }

    public IActionResult GetById(uint id)
    {
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Create(bool createCustomerAfter = false, string? bookingReturnUrl = null)
    {
        try
        {
            var model = await BuildFormModelAsync(new UserFormViewModel
            {
                PageTitle = "Create User",
                IntroText = "Open a new staff or customer-linked account with clear role assignment and operational status from the start.",
                SubmitLabel = "Create User",
                HeroEyebrow = "Account Setup"
            });

            ViewData["CreateCustomerAfter"] = createCustomerAfter;
            ViewData["BookingReturnUrl"] = bookingReturnUrl;

            return View(model);
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model, bool createCustomerAfter = false, string? bookingReturnUrl = null)
    {
        try
        {
            await ValidateUniqueFieldsAsync(model);

            if (!ModelState.IsValid)
            {
                model.PageTitle = "Create User";
                model.IntroText = "Open a new staff or customer-linked account with clear role assignment and operational status from the start.";
                model.SubmitLabel = "Create User";
                model.HeroEyebrow = "Account Setup";
                ViewData["CreateCustomerAfter"] = createCustomerAfter;
                ViewData["BookingReturnUrl"] = bookingReturnUrl;
                return View(await BuildFormModelAsync(model));
            }

            var id = await _service.CreateAsync(new SaveUserDto(model.Username, model.Email, model.PasswordHash, model.RoleId, model.StatusId));

            if (createCustomerAfter)
            {
                var fallback = Url.Action("Create", "Booking") ?? "/Booking/Create";
                var safeReturn = string.IsNullOrWhiteSpace(bookingReturnUrl) ? fallback : bookingReturnUrl;
                var customerCreateUrl = QueryHelpers.AddQueryString(
                    Url.Action("Create", "Customer") ?? "/Customer/Create",
                    new Dictionary<string, string?>
                    {
                        ["userId"] = id.ToString(),
                        ["returnUrl"] = safeReturn
                    });

                return Redirect(customerCreateUrl);
            }

            TempData["SuccessMessage"] = $"User {model.Username} was created successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Update(uint id)
    {
        try
        {
            var user = await _service.GetEditableByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            return View(await BuildFormModelAsync(new UserFormViewModel
            {
                Id = user.id,
                Username = user.username,
                Email = user.email,
                PasswordHash = user.password_hash,
                RoleId = user.role_id,
                StatusId = user.status_id,
                PageTitle = "Update User",
                IntroText = "Adjust account identity, role placement, and lifecycle status while preserving the existing API contract.",
                SubmitLabel = "Save Changes",
                HeroEyebrow = "Account Refresh"
            }));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(uint id, UserFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        try
        {
            await ValidateUniqueFieldsAsync(model, id);

            if (!ModelState.IsValid)
            {
                model.PageTitle = "Update User";
                model.IntroText = "Adjust account identity, role placement, and lifecycle status while preserving the existing API contract.";
                model.SubmitLabel = "Save Changes";
                model.HeroEyebrow = "Account Refresh";
                return View(await BuildFormModelAsync(model));
            }

            await _service.UpdateAsync(id, new SaveUserDto(model.Username, model.Email, model.PasswordHash, model.RoleId, model.StatusId));
            TempData["SuccessMessage"] = $"User {model.Username} was updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Delete(uint id)
    {
        try
        {
            var summary = await _service.GetByIdAsync(id);
            if (summary is null)
            {
                return NotFound();
            }

            var rolesTask = _service.GetRolesAsync();
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(rolesTask, statusesTask);

            return View(MapSummary(summary, rolesTask.Result, statusesTask.Result));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(uint id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetUsernameById(uint id)
    {
        try
        {
            var username = await _service.GetUsernameByIdAsync(id);
            return View("Lookup", new UserLookupResultViewModel
            {
                Title = "Username by Id",
                Description = "Use the direct API helper to confirm the username tied to a specific account id.",
                QueryLabel = "User Id",
                QueryValue = id.ToString(),
                ResultLabel = "Username",
                ScalarResult = username,
                NotFoundMessage = string.IsNullOrWhiteSpace(username) ? "No username was returned for this id." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetUserWithRoleByUsername(string username)
    {
        try
        {
            var user = await _service.GetUserWithRoleByUsernameAsync(username);
            return View("Lookup", BuildLookupUserResult(
                title: "User With Role by Username",
                description: "Inspect the API payload that resolves a user and their assigned role from a username.",
                queryLabel: "Username",
                queryValue: username,
                user));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetUserWithRoleByEmail(string email)
    {
        try
        {
            var user = await _service.GetUserWithRoleByEmailAsync(email);
            return View("Lookup", BuildLookupUserResult(
                title: "User With Role by Email",
                description: "Inspect the API payload that resolves a user and their assigned role from an email address.",
                queryLabel: "Email",
                queryValue: email,
                user));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> UsernameExists(string username)
    {
        try
        {
            var exists = await _service.UsernameExistsAsync(username);
            return View("Lookup", new UserLookupResultViewModel
            {
                Title = "Username Availability",
                Description = "Run the dedicated existence endpoint to verify whether a username is already reserved.",
                QueryLabel = "Username",
                QueryValue = username,
                ResultLabel = "Exists",
                BooleanResult = exists
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> EmailExists(string email)
    {
        try
        {
            var exists = await _service.EmailExistsAsync(email);
            return View("Lookup", new UserLookupResultViewModel
            {
                Title = "Email Availability",
                Description = "Run the dedicated existence endpoint to verify whether an email address is already in use.",
                QueryLabel = "Email",
                QueryValue = email,
                ResultLabel = "Exists",
                BooleanResult = exists
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    private async Task SignInUserAsync(UserEntityDto user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.id.ToString()),
            new(ClaimTypes.Name, user.username),
            new(ClaimTypes.Email, user.email),
            new(ClaimTypes.Role, user.role?.role_name ?? "Customer"),
            new("role_id", user.role_id.ToString()),
            new("status_id", user.status_id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            AllowRefresh = true,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Booking");
    }

    private async Task TryDeleteUserAsync(uint userId)
    {
        try
        {
            await _service.DeleteAsync(userId);
        }
        catch (HttpRequestException)
        {
            // Best-effort rollback if customer creation fails after user creation.
        }
    }

    private static string BuildApiErrorMessage(ApiRequestException ex)
    {
        var summary = $"User service error {(int)ex.HttpStatus} ({ex.HttpStatus}).";
        return string.IsNullOrWhiteSpace(ex.ResponseBody)
            ? summary
            : $"{summary} Details: {ex.ResponseBody}";
    }

    private async Task ValidateUniqueFieldsAsync(UserFormViewModel model, uint? currentId = null)
    {
        var users = await _service.GetAllAsync();

        var usernameTaken = users.Any(user =>
            string.Equals(user.username, model.Username, StringComparison.OrdinalIgnoreCase) &&
            (!currentId.HasValue || user.id != currentId.Value));

        if (usernameTaken)
        {
            ModelState.AddModelError(nameof(model.Username), $"Username '{model.Username}' already exists.");
        }

        var emailTaken = users.Any(user =>
            string.Equals(user.email, model.Email, StringComparison.OrdinalIgnoreCase) &&
            (!currentId.HasValue || user.id != currentId.Value));

        if (emailTaken)
        {
            ModelState.AddModelError(nameof(model.Email), $"Email '{model.Email}' already exists.");
        }
    }

    private async Task<UserFormViewModel> BuildFormModelAsync(UserFormViewModel model)
    {
        var rolesTask = _service.GetRolesAsync();
        var statusesTask = _service.GetStatusesAsync();
        await Task.WhenAll(rolesTask, statusesTask);

        model.RoleOptions = BuildRoleOptions(rolesTask.Result, model.RoleId);
        model.StatusOptions = BuildStatusOptions(statusesTask.Result, model.StatusId);

        if (model.RoleId == 0 && model.RoleOptions.Count > 0 && byte.TryParse(model.RoleOptions[0].Value, out var roleId))
        {
            model.RoleId = roleId;
        }

        if (model.StatusId == 0 && model.StatusOptions.Count > 0 && byte.TryParse(model.StatusOptions[0].Value, out var statusId))
        {
            model.StatusId = statusId;
        }

        return model;
    }

    private IActionResult RedirectToIndexWithApiError()
    {
        TempData["ErrorMessage"] = ApiUnavailableMessage;
        return RedirectToAction(nameof(Index));
    }

    private static UserIndexViewModel BuildUnavailableIndexModel(UserFiltersViewModel filters)
    {
        return new UserIndexViewModel
        {
            Filters = filters,
            Users = [],
            RoleOptions = [],
            StatusOptions = [],
            ActiveFilterSummary = "User API is offline.",
            Stats = new UserIndexStatsViewModel()
        };
    }

    private static List<SelectListItem> BuildRoleOptions(IEnumerable<UserRoleDto> roles, byte? selectedId)
    {
        return roles
            .OrderBy(role => role.role_name, StringComparer.OrdinalIgnoreCase)
            .Select(role => new SelectListItem(role.role_name, role.id.ToString(), role.id == selectedId))
            .ToList();
    }

    private static List<SelectListItem> BuildStatusOptions(IEnumerable<UserStatusDto> statuses, byte? selectedId)
    {
        return statuses
            .OrderBy(status => status.status_name, StringComparer.OrdinalIgnoreCase)
            .Select(status => new SelectListItem(status.status_name, status.id.ToString(), status.id == selectedId))
            .ToList();
    }

    private static string BuildFilterSummary(UserFiltersViewModel filters, IEnumerable<UserRoleDto> roles, IEnumerable<UserStatusDto> statuses)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(filters.Username))
        {
            parts.Add($"username contains '{filters.Username}'");
        }

        if (!string.IsNullOrWhiteSpace(filters.Email))
        {
            parts.Add($"email contains '{filters.Email}'");
        }

        if (filters.RoleId.HasValue)
        {
            var roleName = roles.FirstOrDefault(role => role.id == filters.RoleId.Value)?.role_name;
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                parts.Add($"role is {roleName}");
            }
        }

        if (filters.StatusId.HasValue)
        {
            var statusName = statuses.FirstOrDefault(status => status.id == filters.StatusId.Value)?.status_name;
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                parts.Add($"status is {statusName}");
            }
        }

        if (filters.HasCustomer.HasValue)
        {
            parts.Add(filters.HasCustomer.Value ? "linked customer required" : "without linked customer");
        }

        return parts.Count == 0 ? "Showing the full user directory." : $"Filtered by {string.Join(", ", parts)}.";
    }

    private static UserListItemViewModel MapSummary(
        UserSummaryDto user,
        IEnumerable<UserRoleDto> roles,
        IEnumerable<UserStatusDto> statuses,
        byte? overrideRoleId = null,
        byte? overrideStatusId = null)
    {
        var roleId = overrideRoleId ?? roles.FirstOrDefault(role => string.Equals(role.role_name, user.role, StringComparison.OrdinalIgnoreCase))?.id;
        var statusId = overrideStatusId ?? statuses.FirstOrDefault(status => string.Equals(status.status_name, user.status, StringComparison.OrdinalIgnoreCase))?.id;

        return new UserListItemViewModel
        {
            Id = user.id,
            Username = user.username,
            Email = user.email,
            RoleId = roleId,
            StatusId = statusId,
            RoleName = user.role ?? "Unassigned",
            StatusName = user.status ?? "Unassigned",
            CustomerId = user.customer
        };
    }

    private static string BuildCredentialPreview(string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return "Not available";
        }

        return passwordHash.Length <= 10
            ? passwordHash
            : $"{passwordHash[..6]}...{passwordHash[^4..]}";
    }

    private static UserLookupResultViewModel BuildLookupUserResult(
        string title,
        string description,
        string queryLabel,
        string queryValue,
        UserEntityDto? user)
    {
        return new UserLookupResultViewModel
        {
            Title = title,
            Description = description,
            QueryLabel = queryLabel,
            QueryValue = queryValue,
            ResultLabel = "User payload",
            UserResult = user is null
                ? null
                : new UserLookupUserViewModel
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    RoleId = user.role_id,
                    RoleName = user.role?.role_name ?? "Unassigned",
                    StatusId = user.status_id,
                    CustomerId = user.customer?.id,
                    CreatedAt = user.created_at,
                    UpdatedAt = user.updated_at,
                    HasStoredCredential = !string.IsNullOrWhiteSpace(user.password_hash)
                },
            NotFoundMessage = user is null ? "The API did not return a user for this lookup." : null
        };
    }
}