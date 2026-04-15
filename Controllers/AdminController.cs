using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateLease.Models;

namespace RealEstateLease.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>    _roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Admin ana sayfası: direkt kullanıcı listesine yönlendiriyoruz
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Users));
        }

        // Kullanıcı listesi
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();

            var list = new List<AdminUserVm>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                list.Add(new AdminUserVm
                {
                    Id       = u.Id,
                    Email    = u.Email ?? "",
                    UserName = u.UserName,
                    Roles    = string.Join(", ", roles)
                });
            }

            return View(list);
        }

        // Bir kullanıcıya tek bir rol atama (Admin / Owner / Tenant)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var validRoles = new[] { AppRoles.Admin, AppRoles.Owner, AppRoles.Tenant };
            if (!validRoles.Contains(role))
                return BadRequest("Geçersiz rol");

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"{user.Email} kullanıcısının rolü '{role}' olarak güncellendi.";
            return RedirectToAction(nameof(Users));
        }
    }
}
