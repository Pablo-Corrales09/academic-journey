// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Smart_Gym.Models;

namespace Smart_Gym.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUserStore<Usuario> _userStore;
        private readonly IUserEmailStore<Usuario> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<Usuario> userManager,
            IUserStore<Usuario> userStore,
            SignInManager<Usuario> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage ="Ingrese un correo electrónico válido.")]
            [EmailAddress]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases. 
            /// </summary>
            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La contraseña debe contener al menos {2} como mínimo y {1} como máximo.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Los datos de las contraseñas no cohinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
            [Display(Name = "Nombre")]

            //Datos de la personalizados de la clase Usuario 
            public string Nombre { get; set; }

            [Required(ErrorMessage = "El apellido es obligatorio.")]
            [StringLength(100, MinimumLength = 1, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
            [Display(Name = "Apellido")]
            
            public string Apellido { get; set; }

            
            [StringLength(20, MinimumLength = 1, ErrorMessage = "La cédula no puede exceder los 20 caracteres.")]
            [Display(Name = "Cédula")]
            public string Cedula { get; set; }

            
            [StringLength(200, MinimumLength = 1, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
            [Display(Name = "Dirección")]
            public string Direccion { get; set; }

            
            [StringLength(15, MinimumLength = 1, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
            [Display(Name = "Teléfono")]
            public string Telefono { get; set; }

            
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de nacimiento")]
            public DateTime? FechaNacimiento { get; set; }

            
            [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser un número positivo.")]
            [Display(Name = "Salario")]
            public double? Salario { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid) 
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                //Valores personalizados de la clase Usuario
                user.Nombre = Input.Nombre;
                user.Apellido = Input.Apellido;
                user.Cedula = Input.Cedula;
                user.Direccion = Input.Direccion;
                user.Telefono = Input.Telefono;
                user.FechaNacimiento = Input.FechaNacimiento;
                user.Salario = Input.Salario;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded) { 

                    _logger.LogInformation("Usuario creado con éxito.");

                    // Se declara la variable que se va asignar el rol por defecto "Cliente" al usuario recién creado.
                    var rolDefaultAsignado = await _userManager.AddToRoleAsync(user, "Cliente");

                    //Verifica que la asignación del rol por defecto se haya realizado correctamente, en caso contrario muestra los erroes.
                    if (!rolDefaultAsignado.Succeeded)
                    {
                        foreach (var error in rolDefaultAsignado.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }



                    // Si se requiere la verificación de correo electrónico, envía el correo de confirmación

                    var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirma tu correo electrónico",
                            $"Por favor, confirma tu cuenta haciendo clic aquí: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>enlace</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                            // Si se requiere la confirmación de la cuenta, redirige al usuario a la página de inicio de sesión
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            // Si no se requiere la confirmación de la cuenta, inicia sesión directamente
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                }

         // If we got this far, something failed, redisplay form
            return Page();
        }

        private Usuario CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Usuario>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Usuario)}'. " +
                    $"Ensure that '{nameof(Usuario)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<Usuario> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Usuario>)_userStore;
        }
    }
}
