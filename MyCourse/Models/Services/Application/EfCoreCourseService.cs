using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.Services.Application;
using Microsoft.Data.Sqlite;
using MyCourse.Models.Exceptions;

namespace MyCourse.Models.Services.Application.Courses
{
   
    public class EfCoreCourseService : ICourseService
    {
        
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;

        public EfCoreCourseService(MyCourseDbContext dbContext, IOptionsMonitor<CoursesOptions> coursesOptions)
        {
            this.dbContext = dbContext;
            this.coursesOptions = coursesOptions;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            CourseDetailViewModel viewModel = await dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => new CourseDetailViewModel{
                    Id          = course.Id,
                    Title       = course.Title,
                    Description = course.Description,
                    Author      = course.Author,
                    ImagePath   = course.ImagePath,
                    Rating      = course.Rating,
                    CurrentPrice= course.CurrentPrice,
                    FullPrice   = course.FullPrice, 
                    Lessons     = course.Lessons.Select(lesson => new LessonViewModel {
                        Id          = lesson.Id,
                        Title       = lesson.Title,
                        Description = lesson.Description,
                        Duration    = lesson.Duration
                    }).ToList()
                })
                .SingleAsync();                ;

            return viewModel;
        }

        public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderBy: "Rating",
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
                orderBy: "Id",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }
        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
                        
            IQueryable<Course> baseQuery = dbContext.Courses;

            switch(model.OrderBy)
            {
                case "Title":
                    if (model.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.Title);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course => course.Title);
                    }
                    break;
                case "Rating":
                    if(model.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.Rating);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course => course.Rating);
                    }     
                    break;    
                case "CurrentPrice":
                    if(model.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.CurrentPrice.Amount);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
                    }     
                    break;    
                default:
                    break;

            }

            IQueryable<CourseViewModel> queryLinq = baseQuery
            .Where(course => course.Title.Contains(model.Search))
            .AsNoTracking()
            .Select(course =>
            new CourseViewModel{
                Id          = course.Id,
                Title       = course.Title,
                ImagePath   = course.ImagePath,
                Author      = course.Author,
                Rating      = course.Rating,
                CurrentPrice= course.CurrentPrice,
                FullPrice   = course.FullPrice
            });

            List<CourseViewModel> courses = await queryLinq
            .Skip(model.Offset)
            .Take(model.Limit)            
            .ToListAsync();
            int totalCount = await queryLinq.CountAsync();

            ListViewModel<CourseViewModel> resoult = new ListViewModel<CourseViewModel>
            {
                Results = courses,
                TotalCount = totalCount
            };
            return resoult;
        }

        public async Task<CourseDetailViewModel> CreateCurseAsync(CourseCreateInputModel inputModel)
        {
           // throw new NotImplementedException();
           string title = inputModel.Title;
           string author = "Mario Rossi";
           var course = new Course(title, author);
           dbContext.Add(course);
           await dbContext.SaveChangesAsync();

           return CourseDetailViewModel.FromEntity(course);
        }

        public async Task<bool> IsTitleAviableAsync(string title, int id)
        {
            bool  titleExists = await dbContext.Courses.AnyAsync(course => EF.Functions.Like(course.Title, title));
            return !titleExists;
        }

        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);

            course.ChangeTitle(inputModel.Title);
            course.ChangePrices(inputModel.FullPrice, inputModel.CurrentPrice);
            course.ChangeDescription(inputModel.Description);
            course.ChangeEmail(inputModel.Email);

            string ImagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
            course.ChangeImagePath(ImagePath);

            //dbContext.Update(course)

           try
           {
               await dbContext.SaveChangesAsync();
           }
           catch (DbUpdateException exc ) when ((exc.InnerException as SqliteException)?.SqliteErrorCode ==19 )
           {
               throw new CourseTitleUnaviableException(inputModel.Title, exc);
           } 

           return CourseDetailViewModel.FromEntity(course);
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
                //logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }

            return viewModel;
        }        

    }
}