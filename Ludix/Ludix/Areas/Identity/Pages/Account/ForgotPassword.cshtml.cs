// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Ludix.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var emailBody = GetForgotPasswordEmailBody(callbackUrl, Input.Email);
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Redefinição de Password - Ludix",
                    emailBody);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
        private string GetForgotPasswordEmailBody(string callbackUrl, string email)
        {
            return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #333;'>Redefinição de password</h2>
            <p>Bom dia,</p>
            <p>Recebemos um pedido para redefinir a sua password. Se fez este pedido, clique no botão abaixo:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                   Redifinir password
                </a>
            </div>
            
            <p>Se o botão acima não funcionar, copie e cole este link no seu browser:</p>
            <p style='word-break: break-all; color: #666;'>{HtmlEncoder.Default.Encode(callbackUrl)}</p>
            
            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
            <p style='color: #666; font-size: 12px;'>
                 Se não foi você quem fez este pedido, pode ignorar este email.
            </p>
            <p style='color: #666; font-size: 12px;'>
                Equipa Ludix
            </p>
        </div>";
        }
    }
}
