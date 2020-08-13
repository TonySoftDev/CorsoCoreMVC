﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Services.Application.Courses
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly ILogger<EfCoreCourseService> logger;
        private readonly MyCourseDbContext dbContext;
        private readonly IImagePersister imagePersister;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;

        public EfCoreCourseService(ILogger<EfCoreCourseService> logger, MyCourseDbContext dbContext, IImagePersister imagePersister, IOptionsMonitor<CoursesOptions> coursesOptions)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.imagePersister = imagePersister;
            this.coursesOptions = coursesOptions;
        }

        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            IQueryable<CourseDetailViewModel> queryLinq = dbContext.Courses
                .AsNoTracking()
                .Include(course => course.Lessons)
                .Where(course => course.Id == id)
                .Select(course => CourseDetailViewModel.FromEntity(course)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

            CourseDetailViewModel viewModel = await queryLinq.SingleAsync();
            if (viewModel == null)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }
            //.FirstOrDefaultAsync(); //Restituisce null se l'elenco è vuoto e non solleva mai un'eccezione
            //.SingleOrDefaultAsync(); //Tollera il fatto che l'elenco sia vuoto e in quel caso restituisce null, oppure se l'elenco contiene più di 1 elemento, solleva un'eccezione
            //.FirstAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto solleva un'eccezione

            return viewModel;
        }

        public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Rating",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }

        public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Id",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }


        //public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        //{
        //    IQueryable<Course> baseQuery = dbContext.Courses;

        //    switch (model.OrderBy)
        //    {
        //        case "Title":
        //            if (model.Ascending)
        //            {
        //                baseQuery = baseQuery.OrderBy(course => course.Title);
        //            }
        //            else
        //            {
        //                baseQuery = baseQuery.OrderByDescending(course => course.Title);
        //            }
        //            break;
        //        case "Rating":
        //            if (model.Ascending)
        //            {
        //                baseQuery = baseQuery.OrderBy(course => course.Rating);
        //            }
        //            else
        //            {
        //                baseQuery = baseQuery.OrderByDescending(course => course.Rating);
        //            }
        //            break;
        //        case "CurrentPrice":
        //            if (model.Ascending)
        //            {
        //                baseQuery = baseQuery.OrderBy(course => course.CurrentPrice.Amount);
        //            }
        //            else
        //            {
        //                baseQuery = baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
        //            }
        //            break;
        //        case "Id":
        //            if (model.Ascending)
        //            {
        //                baseQuery = baseQuery.OrderBy(course => course.Id);
        //            }
        //            else
        //            {
        //                baseQuery = baseQuery.OrderByDescending(course => course.Id);
        //            }
        //            break;
        //    }

        //    IQueryable<CourseViewModel> queryLinq = baseQuery
        //        .Where(course => course.Title.Contains(model.Search))
        //        .Skip(model.Offset)
        //        .Take(model.Limit)
        //        .AsNoTracking()
        //        .Select(course => CourseViewModel.FromEntity(course)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

        //    List<CourseViewModel> courses = await queryLinq
        //        .Skip(model.Offset)
        //        .Take(model.Limit)
        //        .ToListAsync(); //La query al database viene inviata qui, quando manifestiamo l'intenzione di voler leggere i risultati

        //    int totalCount = await queryLinq.CountAsync();

        //    ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
        //    { 
        //        Results = courses,
        //        TotalCount = totalCount
        //    };

        //    return result;
        //}

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            IQueryable<Course> baseQuery = dbContext.Courses;

            baseQuery = (model.OrderBy, model.Ascending) switch
            {
                ("Title", true) => baseQuery.OrderBy(course => course.Title),
                ("Title", false) => baseQuery.OrderByDescending(course => course.Title),
                ("Rating", true) => baseQuery.OrderBy(course => course.Rating),
                ("Rating", false) => baseQuery.OrderByDescending(course => course.Rating),
                ("CurrentPrice", true) => baseQuery.OrderBy(course => course.CurrentPrice.Amount),
                ("CurrentPrice", false) => baseQuery.OrderByDescending(course => course.CurrentPrice.Amount),
                ("Id", true) => baseQuery.OrderBy(course => course.Id),
                ("Id", false) => baseQuery.OrderByDescending(course => course.Id),
                _ => baseQuery
            };

            IQueryable<Course> queryLinq = baseQuery
                .Where(course => course.Title.Contains(model.Search))
                .AsNoTracking();

            List<CourseViewModel> courses = await queryLinq
                .Skip(model.Offset)
                .Take(model.Limit)
                .Select(course => CourseViewModel.FromEntity(course)) //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato
                .ToListAsync(); //La query al database viene inviata qui, quando manifestiamo l'intenzione di voler leggere i risultati

            int totalCount = await queryLinq.CountAsync();

            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courses,
                TotalCount = totalCount
            };

            return result;
        }

        public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
        {
            string title = inputModel.Title;
            string author = "Mario Rossi";

            var course = new Course(title, author);
            dbContext.Add(course);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(title, exc);
            }

            return CourseDetailViewModel.FromEntity(course);
        }

        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);

            if (course == null)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }

            course.ChangeTitle(inputModel.Title);
            course.ChangePrices(inputModel.FullPrice, inputModel.CurrentPrice);
            course.ChangeDescription(inputModel.Description);
            course.ChangeEmail(inputModel.Email);

            dbContext.Entry(course).Property(course => course.RowVersion).OriginalValue = inputModel.RowVersion;

            if (inputModel.Image != null)
            {
                try
                {
                    string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                    course.ChangeImagePath(imagePath);
                }
                catch (Exception exc)
                {
                    throw new CourseImageInvalidException(inputModel.Id, exc);
                }
            }

            dbContext.Update(course);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }


            return CourseDetailViewModel.FromEntity(course);
        }


        public async Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            //await dbContext.Courses.AnyAsync(course => course.Title == title);
            bool titleExists = await dbContext.Courses.AnyAsync(course => EF.Functions.Like(course.Title, title) && course.Id != id);
            return !titleExists;
        }

        public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            IQueryable<CourseEditInputModel> queryLinq = dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => CourseEditInputModel.FromEntity(course)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

            CourseEditInputModel viewModel = await queryLinq.FirstOrDefaultAsync();

            if (viewModel == null)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }

            return viewModel;
        }

        public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);

            if (course == null)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }

            course.ChangeStatus(CourseStatus.Deleted);
            await dbContext.SaveChangesAsync();
        }

    }
}
