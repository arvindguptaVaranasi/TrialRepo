﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BLServices;
using Model;
using EmployeeManagement.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {

        private IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;


        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment)
        {
            this._employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }
        public ViewResult Index()
        {
            // retrieve all the employees
            var model = _employeeRepository.GetAllEmployee();
            // Pass the list of employees to the view
            return View(model);            
            //return _employeeRepository.GetEmployee(1).Name;
        }
        public ViewResult Details(int id)
        {
            throw new Exception("Error in Details View");
            //Employee model = _employeeRepository.GetEmployee(id);
            //ViewBag.PageTitle = "Employee Details";

            //// Instantiate HomeDetailsViewModel and store Employee details and PageTitle
            //HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            //{
            //    Employee = _employeeRepository.GetEmployee(id),               
            //    PageTitle = "Employee Details"
            //};
            //if (homeDetailsViewModel.Employee == null)
            //{
            //    Response.StatusCode = 404;
            //    return View("EmployeeNotFound", id);

            //}
            //return View(homeDetailsViewModel);
        }
        [HttpPost]
        public IActionResult Create(CreateViewModel model)
        {
            string uniqueFileName = null;

            // If the Photo property on the incoming model object is not null, then the user
            // has selected an image to upload.
            if (model.Photo != null)
            {
                // Loop thru each selected file
                foreach (IFormFile photo in model.Photo)
                {
                    // The file must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the injected
                    // IHostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }
            }

            Employee newEmployee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                Department = model.Department,
                // Store the file name in PhotoPath property of the employee object
                // which gets saved to the Employees database table
                PhotoPath = uniqueFileName
            };

            _employeeRepository.Add(newEmployee);
            return RedirectToAction("details", new { id = newEmployee.Id });            
        }
        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }
        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);

            }
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }
        // Through model binding, the action method parameter
        // EmployeeEditViewModel receives the posted edit form data
        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            // Check if the provided data is valid, if not rerender the edit view
            // so the user can correct and resubmit the edit form
            if (ModelState.IsValid)
            {
                // Retrieve the employee being edited from the database
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                // If the user wants to change the photo, a new photo will be
                // uploaded and the Photo property on the model object receives
                // the uploaded photo. If the Photo property is null, user did
                // not upload a new photo and keeps his existing photo
                if (model.Photo != null)
                {
                    // If a new photo is uploaded, the existing photo must be
                    // deleted. So check if there is an existing photo and delete
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                            "Images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    // Save the new photo in wwwroot/images folder and update
                    // PhotoPath property of the employee object which will be
                    // eventually saved in the database
                    employee.PhotoPath = ProcessUploadedFile(model);
                }
                Employee UpdateEmployee = _employeeRepository.Update(employee);
                return RedirectToAction("index");

            }


            return View(model);

        }
        private string ProcessUploadedFile(CreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {               
                foreach (IFormFile photo in model.Photo)
                {
                    // The file must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the injected
                    // IHostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }

            }
            return uniqueFileName;
        }

        //public JsonResult Index()
        //{
        //    return Json(new { id = 1, name = "pragim" });
        //}
    }
}