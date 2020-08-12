﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Home;

namespace MyCourse.Controllers
{
    public class HomeController : Controller
    {
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [ResponseCache(CacheProfileName = "Home")]
        public async Task<IActionResult> Index([FromServices] ICachedCourseService courseService)
        {
            ViewData["Title"] = "Home Page WebApp";
            List<CourseViewModel> bestRatingCourses = await courseService.GetBestRatingCoursesAsync();
            List<CourseViewModel> mostRecentCourses = await courseService.GetMostRecentCoursesAsync();

            HomeViewModel viewModel = new HomeViewModel
            {
                BestRatingCourses = bestRatingCourses,
                MostRecentCourses = mostRecentCourses
            };

            return View(viewModel);
        }
    }
}
