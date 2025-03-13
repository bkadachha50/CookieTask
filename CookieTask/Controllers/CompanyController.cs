using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CompanyTask.Models;

namespace CompanyTask.Controllers
{
    public class CompanyController : Controller
    {
        private const string CookieCompanyList = "CompanyList";

        private List<CompanyModel> GetCompaniesFromCookie()
        {
            var cookie = Request.Cookies[CookieCompanyList];
            return string.IsNullOrEmpty(cookie) ? new List<CompanyModel>() : JsonConvert.DeserializeObject<List<CompanyModel>>(cookie);
        }

        private void SaveCompaniesToCookie(List<CompanyModel> companies)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5),
                HttpOnly = true
            };
            Response.Cookies.Append(CookieCompanyList, JsonConvert.SerializeObject(companies), options);
        }

        public IActionResult Index()
        {
            var companies = GetCompaniesFromCookie().Where(c => !c.IsDeleted).ToList();
            return View(companies);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CompanyModel model)
        {
            if (ModelState.IsValid)
            {
                var companies = GetCompaniesFromCookie();
                model.Id = companies.Any() ? companies.Max(c => c.Id) + 1 : 1;
                companies.Add(model);
                SaveCompaniesToCookie(companies);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var company = GetCompaniesFromCookie().FirstOrDefault(c => c.Id == id);
            if (company == null) return NotFound();
            return View(company);
        }

        [HttpPost]
        public IActionResult Edit(CompanyModel model)
        {
            if (ModelState.IsValid)
            {
                var companies = GetCompaniesFromCookie();
                var company = companies.FirstOrDefault(c => c.Id == model.Id);
                if (company == null) return NotFound();

                company.CompanyName = model.CompanyName;
                company.Startdate = model.Startdate;
                company.IsActive = model.IsActive;

                SaveCompaniesToCookie(companies);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var companies = GetCompaniesFromCookie();
            var company = companies.FirstOrDefault(c => c.Id == id);
            if (company == null) return NotFound();

            company.IsDeleted = true;
            company.IsActive = false;

            SaveCompaniesToCookie(companies);
            return RedirectToAction("Index");
        }

        public IActionResult DeletedCompany()
        {
            var companies = GetCompaniesFromCookie().Where(c => c.IsDeleted).ToList();
            return View(companies);
        }
    }
}
