﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService courseService;

        public CoursesController(ICachedCourseService courseService)
        {
            this.courseService = courseService;
        }
        public async Task<IActionResult> Index(CourseListInputModel input)
        {
            ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(input);
            ViewData["Title"] = "Catalogo dei corsi";

            CourseListViewModel viewModel = new CourseListViewModel
            {
                Courses = courses,
                Input = input
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
            ViewData["Title"] = viewModel.Title;
            return View(viewModel);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Nuovo corso";
            var inputModel = new CourseCreateInputModel();
            return View(inputModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CourseDetailViewModel course = await courseService.CreateCourseAsync(inputModel);
                    TempData["ConfirmationMessage"] = "Ok! Il tuo corso è stato creato, ora perché non inserisci anche gli altri dati?";
                    return RedirectToAction(nameof(Edit), new { id = course.Id });
                }
                catch (CourseTitleUnavailableException)
                {
                    ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo già esiste");
                }
            }

            ViewData["Title"] = "Nuovo corso";
            return View(inputModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifica corso";
            CourseEditInputModel inputModel = await courseService.GetCourseForEditingAsync(id);
            return View(inputModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CourseEditInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CourseDetailViewModel course = await courseService.EditCourseAsync(inputModel);
                    TempData["ConfirmationMessage"] = "I dati sono stati salvati con successo";
                    return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
                }
                catch (CourseImageInvalidException)
                {
                    ModelState.AddModelError(nameof(CourseEditInputModel.Image), "L'immagine selezionata non è valida");
                }
                catch (CourseTitleUnavailableException)
                {
                    ModelState.AddModelError(nameof(CourseEditInputModel.Title), "Questo titolo già esiste");
                }
                catch (OptimisticConcurrencyException)
                {
                    ModelState.AddModelError("", "Spiacenti, il salvataggio non è andato a buon fine poiché nel frattempo un altro utente ha aggiornato il corso. " +
                                                 "Puoi aggiornare la pagina e ripetere le modifiche");
                }
            }

            ViewData["Title"] = "Modifica corso";
            return View(inputModel);
        }

        public async Task<IActionResult> IsTitleAvailable(string title, int id = 0)
        {
            bool result = await courseService.IsTitleAvailableAsync(title, id);
            return Json(result);
        }

    }
}
