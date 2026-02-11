using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DigitalSignage.Services;
using DigitalSignage.ViewModels;
using DigitalSignage.DTOs;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(
            IUserService userService,
            IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        // GET: User
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var pagedResult = await _userService.GetUsersPagedAsync(page, pageSize);
                var viewModels = _mapper.Map<List<UserViewModel>>(pagedResult.Items);

                ViewBag.TotalCount = pagedResult.TotalCount;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error loading users: {ex.Message}");
                return View(new List<UserViewModel>());
            }
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userService.GetUserWithRolesAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("common.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error loading user details: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: User/Create
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Create(CreateUserDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);

                var user = await _userService.CreateAsync(_mapper.Map<Models.User>(dto));

                AddSuccessMessage(T("common.createSuccess"));
                return RedirectToAction(nameof(Details), new { id = user.UserID });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error creating user: {ex.Message}");
                return View(dto);
            }
        }

        // GET: User/Edit/5
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("common.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error loading user: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Edit(int id, UserViewModel viewModel)
        {
            try
            {
                if (id != viewModel.UserID)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(viewModel);

                var user = _mapper.Map<Models.User>(viewModel);
                await _userService.UpdateAsync(user);

                AddSuccessMessage(T("common.updateSuccess"));
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error updating user: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: User/Delete/5
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("common.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error loading user: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                AddSuccessMessage(T("common.deleteSuccess"));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error deleting user: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string currentPassword, string newPassword)
        {
            try
            {
                var success = await _userService.ChangePasswordAsync(id, currentPassword, newPassword);

                if (!success)
                {
                    AddErrorMessage("Current password is incorrect");
                    return RedirectToAction(nameof(Details), new { id });
                }

                AddSuccessMessage("Password changed successfully");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error changing password: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
