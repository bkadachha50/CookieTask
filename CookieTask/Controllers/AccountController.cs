using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CompanyTask.Models;
using CompanyTask.Extensions;

namespace CompanyTask.Controllers
{
    public class AccountController : Controller
    {
        private const string CookieUserKey = "LoggedInUser";
        private const string CookieUserList = "UserList";

        private bool IsUserLoggedIn() => Request.Cookies[CookieUserKey] != null;

        [HttpGet]
        public IActionResult Register()
        {
            if (IsUserLoggedIn())
                return RedirectToAction("Index", "Company");
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var userList = Request.Cookies.GetObjectFromJson<List<UserModel>>(CookieUserList) ?? new List<UserModel>();

                if (userList.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                model.Id = userList.Any() ? userList.Max(u => u.Id) + 1 : 1;
                userList.Add(model);

                Response.Cookies.SetObjectAsJson(CookieUserList, userList, 7);
                TempData["Success"] = "Registration successful. Please login.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsUserLoggedIn())
                return RedirectToAction("Index", "Company");
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserModel model)
        {
            var userList = Request.Cookies.GetObjectFromJson<List<UserModel>>(CookieUserList) ?? new List<UserModel>();

            var user = userList.FirstOrDefault(u =>
                u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == model.Password
            );

            if (user != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(5),
                    HttpOnly = true, 
                    Secure = true,   
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append(CookieUserKey, user.Email, cookieOptions);
                return RedirectToAction("Index", "Company");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete(CookieUserKey);
            return RedirectToAction("Login");
        }

        public IActionResult UserList()
        {
            var userList = Request.Cookies.GetObjectFromJson<List<UserModel>>(CookieUserList) ?? new List<UserModel>();
            return View(userList);
        }
    }
}
