using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.ComponentModel.DataAnnotations;
using YoutubeBlog.Entity.DTOs.Articles;
using YoutubeBlog.Entity.DTOs.Categories;
using YoutubeBlog.Entity.Entities;
using YoutubeBlog.Service.Services.Abstraction;
using YoutubeBlog.Service.Services.Concrete;
using YoutubeBlog.Web.ResultMessages;
using static YoutubeBlog.Web.ResultMessages.Messages;

namespace YoutubeBlog.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;
        private readonly IValidator<Entity.Entities.Category> validator;
        private readonly IToastNotification toastNotification;
        public CategoryController(ICategoryService categoryService, IValidator<Entity.Entities.Category> validator, IMapper mapper, IToastNotification toastNotification)
        {
            this.categoryService = categoryService;
            this.validator = validator;
            this.mapper = mapper;
            this.toastNotification = toastNotification;
        }


        public async Task<IActionResult> Index()
        {
            var categories = await categoryService.GetAllCategoriesNonDeleted();
            return View(categories);
        }
        public async Task<IActionResult> DeletedCategory()
        {
            var categories = await categoryService.GetAllCategoriesDeleted();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(CategoryAddDto categoryAddDto)
        {
            var map = mapper.Map<Entity.Entities.Category>(categoryAddDto);
            var result = await validator.ValidateAsync(map);
            if (result.IsValid)
            {
                await categoryService.CreateCategoryAsync(categoryAddDto);
                toastNotification.AddSuccessToastMessage(Messages.Category.Add(categoryAddDto.Name), new ToastrOptions { Title = "Başarılı" });
                return RedirectToAction("Index", "Category", new { Area = "Admin" });
            }

            result.AddToModelState(this.ModelState);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddWithAjax([FromBody] CategoryAddDto categoryAddDto)
        {
            var map = mapper.Map<Entity.Entities.Category>(categoryAddDto);
            var result = await validator.ValidateAsync(map);
            if (result.IsValid)
            {
                await categoryService.CreateCategoryAsync(categoryAddDto);
                toastNotification.AddSuccessToastMessage(Messages.Category.Add(categoryAddDto.Name), new ToastrOptions { Title = "Başarılı" });
                return Json(Messages.Category.Add(categoryAddDto.Name));
            }
            else
            {
                toastNotification.AddErrorToastMessage(result.Errors.First().ErrorMessage,new ToastrOptions { Title = "İşlem Başarısız" });
                return Json(result.Errors.First().ErrorMessage);

            }
        }
        [HttpGet]
        public async Task <IActionResult> Update(Guid categoryId)
        {
            var category = await categoryService.GetCategoryByGuid(categoryId);
            var map = mapper.Map<Entity.Entities.Category, CategoryUpdateDto>(category);
            return View(map);
        }
        [HttpPost]
        public async Task<IActionResult> Update(CategoryUpdateDto categoryUpdateDto)
        {
            var map = mapper.Map<Entity.Entities.Category>(categoryUpdateDto);
            var result = await validator.ValidateAsync(map);
            if (result.IsValid)
            {
                var name = await categoryService.UpdateCategoryAsync(categoryUpdateDto);
                toastNotification.AddSuccessToastMessage(Messages.Category.Update(name), new ToastrOptions() { Title = "İşlem başarılı" });
                return RedirectToAction("Index", "Category", new { Area = "Admin" });
            }
            return View(map);
        }
        public async Task<IActionResult> Delete(Guid categoryId)
        {
            var title = await categoryService.SafeDeleteArticleAsync(categoryId);
            toastNotification.AddSuccessToastMessage(Messages.Category.Delete(title), new ToastrOptions() { Title = "İşlem başarılı" });
            return RedirectToAction("Index", "Category", new { Area = "Admin" });
        }
        public async Task<IActionResult> UndoDelete(Guid categoryId)
        {
            var title = await categoryService.UndoDeleteArticleAsync(categoryId);
            toastNotification.AddSuccessToastMessage(Messages.Category.UndoDelete(title), new ToastrOptions() { Title = "İşlem başarılı" });
            return RedirectToAction("Index", "Category", new { Area = "Admin" });
        }
    }
}
