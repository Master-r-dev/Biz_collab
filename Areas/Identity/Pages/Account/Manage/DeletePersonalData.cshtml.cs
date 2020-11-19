using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Biz_collab.Data;
using Biz_collab.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Biz_collab.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly ApplicationDbContext _db;
        public DeletePersonalDataModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _db = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);           
            var userId = await _userManager.GetUserIdAsync(user);
            var client = await _db.Clients.FindAsync(userId);
            /* Если не происходит автоматически-вернуть,иначе-удалить!
            //удалить его группы
            
            var groups =  _db.Groups.Include(m => m.Clients.Where(m=>m.Client.Id ==client.Id && m.ClientRole == "Создатель"));
            foreach ( var g in groups)
            {
                var transaction = _db.Transactions.Include(t => t.GroupId == g.Id);
                _db.Remove(transaction);
            }
            //убрать его из участников в других группах
            var groups_s=  _db.Groups.Include(m => m.Clients.Where(p => p.Client.Id == client.Id));
            foreach (var g in groups_s)
            {
                var transaction = _db.Transactions.Include(t => t.GroupId == g.Id && t.ClientId == client.Id);
                _db.Remove(transaction);
            }
            _db.Remove(groups);
            _db.Remove(groups_s);*/            
            _db.Clients.Remove(client);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();
            await _db.SaveChangesAsync();
            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}
