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
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index(string searchString, string roleFilter)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            // Apply search filter (Name, Email, Username)
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FullName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.UserName.Contains(searchString));
            }

            var users = usersQuery.Select(u => new AdminUserViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                UserName = u.UserName,
                RegistrationDate = u.RegistrationDate
            }).ToList();

            // Apply role filter
            if (!string.IsNullOrEmpty(roleFilter))
            {
                var filteredUsers = new List<AdminUserViewModel>();
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.Id);
                    var roles = await _userManager.GetRolesAsync(appUser);
                    if (roles.Contains(roleFilter))
                    {
                        user.Role = roles.FirstOrDefault();
                        filteredUsers.Add(user);
                    }
                }
                users = filteredUsers;
            }
            else
            {
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.Id);
                    var roles = await _userManager.GetRolesAsync(appUser);
                    user.Role = roles.FirstOrDefault();
                }
            }

            // Pass roles for dropdown
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentRoleFilter = roleFilter;

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
            ModelState.Remove("Id");
            ModelState.Remove("UserName");

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    ViewBag.Roles = await GetNonAdminRoles();
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    UserName = model.Email,
                    NormalizedUserName = model.Email.ToUpper(),
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpper(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RegistrationDate = DateTime.Now,
                    Age = model.Age ?? 0,
                    IDNumber = model.IDNumber ?? string.Empty,
                    City = model.City ?? string.Empty,
                    CampusName = model.CampusName ?? string.Empty
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, $"Role Error: {error.Description}");
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, GetFriendlyErrorMessage(error));
                }
            }

            ViewBag.Roles = await GetNonAdminRoles();
            return View(model);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(string id)
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
                Age = user.Age > 0 ? user.Age : null,
                IDNumber = user.IDNumber,
                City = user.City,
                CampusName = user.CampusName
            };

            ViewBag.Roles = await GetNonAdminRoles();
            return View(model);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("UserName");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Check if email is being changed and if it's already taken
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != id)
                    {
                        ModelState.AddModelError("Email", "This email is already registered to another user");
                        ViewBag.Roles = await GetNonAdminRoles();
                        return View(model);
                    }
                }

                // Update user properties
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedUserName = model.Email.ToUpper();
                user.NormalizedEmail = model.Email.ToUpper();
                user.Age = model.Age ?? 0;
                user.IDNumber = model.IDNumber ?? string.Empty;
                user.City = model.City ?? string.Empty;
                user.CampusName = model.CampusName ?? string.Empty;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ViewBag.Roles = await GetNonAdminRoles();
                    return View(model);
                }

                // Update role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.FirstOrDefault() != model.Role)
                {
                    // Remove existing roles
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        ViewBag.Roles = await GetNonAdminRoles();
                        return View(model);
                    }

                    // Add new role
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        if (!addRoleResult.Succeeded)
                        {
                            foreach (var error in addRoleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            ViewBag.Roles = await GetNonAdminRoles();
                            return View(model);
                        }
                    }
                }

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await GetNonAdminRoles();
            return View(model);
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
                Age = user.Age > 0 ? user.Age : null,
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
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
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
    }
}