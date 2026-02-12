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
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
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
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoadingDetails"));
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

                AddSuccessMessage(T("user.createdSuccess"));
                return RedirectToAction(nameof(Details), new { id = user.UserID });
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorCreating"));
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
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var dto = _mapper.Map<UpdateUserDTO>(user);
                return View(dto);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Edit(int id, UpdateUserDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                _mapper.Map(dto, user);
                await _userService.UpdateAsync(user);

                AddSuccessMessage(T("user.updatedSuccess"));
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorUpdating"));
                return View(dto);
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
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
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
                AddSuccessMessage(T("user.deletedSuccess"));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorDeleting"));
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
                    AddErrorMessage(T("user.passwordIncorrect"));
                    return RedirectToAction(nameof(Details), new { id });
                }

                AddSuccessMessage(T("user.passwordChanged"));
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorChangingPassword"));
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
