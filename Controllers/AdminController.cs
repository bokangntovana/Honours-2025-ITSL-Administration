using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ITSL_Administration.Models;
using ITSL_Administration.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.Select(u => new AdminUserViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                UserName = u.UserName,
                RegistrationDate = u.RegistrationDate
            }).ToList();

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(appUser);
                user.Role = roles.FirstOrDefault();
            }

            return View(users);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles
                .Where(r => r.Name != "Admin")
                .Select(r => r.Name).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserViewModel model)
        {
            // Clear validation for fields we handle manually
            ModelState.Remove("Id");
            ModelState.Remove("UserName");

            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    ViewBag.Roles = await GetNonAdminRoles();
                    return View(model);
                }

                // Create user exactly like in your seed example
                var user = new Users
                {
                    FullName = model.FullName,
                    UserName = model.Email,
                    NormalizedUserName = model.Email.ToUpper(),
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpper(),
                    EmailConfirmed = true, // Set to true if you don't require email confirmation
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RegistrationDate = DateTime.Now,
                    // Optional fields from your model
                    Age = model.Age,
                    IDNumber = model.IDNumber,
                    City = model.City,
                    CampusName = model.CampusName
                };

                // Create user with password
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role if specified
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        if (!roleResult.Succeeded)
                        {
                            // If role assignment fails, log errors but keep the user
                            foreach (var error in roleResult.Errors)
                            {
                                Console.WriteLine($"Role Error: {error.Code} - {error.Description}");
                                ModelState.AddModelError(string.Empty, $"Role Error: {error.Description}");
                            }
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }

                // Log and display creation errors
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Role Error: {error.Code} - {error.Description}");
                    ModelState.AddModelError(string.Empty, GetFriendlyErrorMessage(error));
                }
            }

            ViewBag.Roles = await GetNonAdminRoles();
            return View(model);
        }

        private string GetFriendlyErrorMessage(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateUserName" => "This username is already taken",
                "DuplicateEmail" => "This email is already registered",
                "PasswordTooShort" => "Password must be at least 8 characters",
                "PasswordRequiresNonAlphanumeric" => "Password requires at least one special character",
                "PasswordRequiresDigit" => "Password requires at least one number",
                "PasswordRequiresUpper" => "Password requires at least one uppercase letter",
                _ => error.Description
            };
        }

        private async Task<List<string>> GetNonAdminRoles()
        {
            return (await _roleManager.Roles
                .Where(r => r.Name != "Admin")
                .ToListAsync())
                .Select(r => r.Name)
                .ToList();
        }
        // GET: Admin/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new AdminUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                RegistrationDate = user.RegistrationDate,
                Role = roles.FirstOrDefault(),
                Age = user.Age,
                IDNumber = user.IDNumber,
                City = user.City,
                CampusName = user.CampusName
            };

            return View(model);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate
            };

            return View(model);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
