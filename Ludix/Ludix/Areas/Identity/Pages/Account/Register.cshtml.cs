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
using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Ludix.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly LudixContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            LudixContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Nome de utilizador")]
            [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
            public string Username { get; set; }

            [Display(Name = "Solicitar ser desenvolvedor")]
            public bool RequestDeveloper { get; set; }

            [Display(Name = "Website (para desenvolvedores)")]
            public string Website { get; set; }
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
                var identityUser = CreateUser();

                await _userStore.SetUserNameAsync(identityUser, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(identityUser, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(identityUser, Input.Password);

                if (result.Succeeded)
                {
                    bool existsError = false;

                    // Criar utilizador normal (todos começam como MyUser)
                    var myUser = new MyUser
                    {
                        AspUser = identityUser.Id,
                        Email = Input.Email,
                        Username = Input.Username,
                        CreatedAt = DateTime.Now,
                        Balance = 0,
                        IsAdmin = false // Por padrão, não é admin
                    };

                    // Se solicitou ser desenvolvedor, adiciona os campos de solicitação
                    if (Input.RequestDeveloper)
                    {
                        myUser.RequestedDeveloper = true;
                        myUser.ProposedWebsite = Input.Website;
                        myUser.DeveloperRequestDate = DateTime.Now;
                    }

                    try
                    {
                        _context.Add(myUser);
                        await _context.SaveChangesAsync();

                        // Se for o primeiro utilizador registado, torná-lo administrador
                        if (_context.MyUser.Count() == 1)
                        {
                            myUser.IsAdmin = true;
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        existsError = true;
                        _logger.LogError(ex, "Erro ao criar utilizador");
                        ModelState.AddModelError(string.Empty, "Erro na criação do utilizador.");
                    }

                    if (!existsError)
                    {
                        var userId = await _userManager.GetUserIdAsync(identityUser);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);
                        await _emailSender.SendEmailAsync(Input.Email, "Confirme a sua conta - Sistema Ludix",
                            GetConfirmationEmailBody(callbackUrl, Input.Email));

                        if (Input.RequestDeveloper)
                        {
                            TempData["Message"] = "O seu pedido para se tornar desenvolvedor foi enviado. Um administrador irá analisar a sua solicitação.";
                        }

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(identityUser, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }

        private string GetConfirmationEmailBody(string callbackUrl, string email)
        {
            return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #333;'>Bem-vindo ao Ludix!</h2>
            <p>Bom dia,</p>
            <p>Obrigado por se registar no Ludix. Para concluir o seu registo, confirme o seu endereço de email.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                   Confirmar Email
                </a>
            </div>
            
            <p>Se o botão acima não funcionar, copie e cole este link no seu browser:</p>
            <p style='word-break: break-all; color: #666;'>{HtmlEncoder.Default.Encode(callbackUrl)}</p>
            
            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
            <p style='color: #666; font-size: 12px;'>
                Se não se registou no Ludix, pode ignorar este email.
            </p>
            <p style='color: #666; font-size: 12px;'>
                Equipa Ludix
            </p>
        </div>";
        }
    }
}