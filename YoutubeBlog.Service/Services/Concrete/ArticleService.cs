using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using YoutubeBlog.Data.UnitOfWorks;
using YoutubeBlog.Entity.DTOs.Articles;
using YoutubeBlog.Entity.Entities;
using YoutubeBlog.Entity.Enums;
using YoutubeBlog.Service.Extensions;
using YoutubeBlog.Service.Helpers.Images;
using YoutubeBlog.Service.Services.Abstraction;

namespace YoutubeBlog.Service.Services.Concrete
{
    public class ArticleService : IArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IImageHelper ımageHelper;
        private readonly ClaimsPrincipal _user;
        public ArticleService(IUnitOfWork unitOfWork, IMapper mapper,IHttpContextAccessor httpContextAccessor,IImageHelper ımageHelper)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;

            this.ımageHelper = ımageHelper;
            _user = httpContextAccessor.HttpContext.User;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<ArticleListDto> GetAllPagingAsync(Guid? categoryId,int currentPage=1,int pageSize=3,bool isAscending = false)
        {
            pageSize = pageSize > 20 ? 20 : pageSize;

            var article = categoryId == null
                ? await _unitOfWork.GetRepository<Article>().GetAllAsync(a => !a.IsDeleted, a => a.Category, i => i.Image)
                : await _unitOfWork.GetRepository<Article>().GetAllAsync(a=>a.CategoryId==categoryId && !a.IsDeleted,x=>x.Category, i => i.Image);
            var sortedArticles = isAscending
                ? article.OrderBy(x => x.CreatedDate).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList()
                : article.OrderByDescending(x => x.CreatedDate).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            return new ArticleListDto
            {
                Articles = sortedArticles,
                CategoryId = categoryId == null ? null : categoryId.Value,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = article.Count,
                IsAscending = isAscending
            };
        }
        public async Task CreateArticleAsync(ArticleAddDto articleAddDto)
        {
            //var userId = Guid.Parse("ıd");
            var user = _user.GetLoggedInUserId();
            var email = _user.GetLoggedInEmail();

            var imageUpload = await ımageHelper.Upload(articleAddDto.Title,articleAddDto.Photo,ImageType.Post);
            Image image = new(imageUpload.FullName,articleAddDto.Photo.ContentType,email);
            await _unitOfWork.GetRepository<Image>().AddAsync(image);


            var article = new Article
            {
                Title = articleAddDto.Title,
                Content = articleAddDto.Content,
                CategoryId = articleAddDto.CategoryId,
                UserId= user,
                ImageId=image.Id,
                CreatedBy=email
            };
            await _unitOfWork.GetRepository<Article>().AddAsync(article);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<ArticleDto>> GetAllArticlesWithCategoryNonDeletedAsync()
        {

            var articles = await _unitOfWork.GetRepository<Article>().GetAllAsync(x=>!x.IsDeleted,x=>x.Category,y=>y.Image);
            var map = mapper.Map<List<ArticleDto>>(articles);
            return map;
        }

        public async Task<List<ArticleDto>> GetAllDeletedArticles()
        {
            var articles = await _unitOfWork.GetRepository<Article>().GetAllAsync(x => x.IsDeleted, x => x.Category, y => y.Image);
            var map = mapper.Map<List<ArticleDto>>(articles);
            return map;
        }

        public async Task<ArticleDto> GetArticlesWithCategoryNonDeletedAsync(Guid articleId)
        {

            var article = await _unitOfWork.GetRepository<Article>().GetAsync(x => !x.IsDeleted && x.Id==articleId, x => x.Category,y=>y.Image);
            var map = mapper.Map<ArticleDto>(article);
            return map;
        }

        public async Task<string> SafeDeleteArticleAsync(Guid articleId)
        {
            var userEmail = _user.GetLoggedInEmail();
            var article = await _unitOfWork.GetRepository<Article>().GetByGuidAsync(articleId);
            article.IsDeleted = true;
            article.DeletedDate = DateTime.Now;
            article.DeletedBy = userEmail;
            await _unitOfWork.GetRepository<Article>().UpdateAsync(article);
            await _unitOfWork.SaveAsync();
            return article.Title;
        }

        public async Task<string> UndoDeleteArticleAsync(Guid articleId)
        {
            var userEmail = _user.GetLoggedInEmail();
            var article = await _unitOfWork.GetRepository<Article>().GetByGuidAsync(articleId);
            article.IsDeleted = false;
            article.DeletedDate =null;
            article.DeletedBy = null;
            await _unitOfWork.GetRepository<Article>().UpdateAsync(article);
            await _unitOfWork.SaveAsync();
            return article.Title;
        }

        public async Task<string> UpdateArticleAsync(ArticleUpdateDto articleUpdateDto)
        {
            var userEmail = _user.GetLoggedInEmail();

            var article= await _unitOfWork.GetRepository<Article>().GetAsync(x => !x.IsDeleted && x.Id == articleUpdateDto.Id, x => x.Category, y => y.Image);

            if (articleUpdateDto.Photo!=null)
            {
                ımageHelper.Delete(article.Image.FileName);
                var imageUpload = await ımageHelper.Upload(articleUpdateDto.Title, articleUpdateDto.Photo, ImageType.Post);
                Image image = new Image(imageUpload.FullName,articleUpdateDto.Photo.ContentType,userEmail);
                await _unitOfWork.GetRepository<Image>().AddAsync(image);
                article.ImageId = image.Id;
            }


            article.Title = articleUpdateDto.Title;
            article.Content = articleUpdateDto.Content;
            article.CategoryId = articleUpdateDto.CategoryId;
            article.ModifiedDate = DateTime.Now;
            article.ModifiedBy = userEmail;
            await _unitOfWork.GetRepository<Article>().UpdateAsync(article);
            await _unitOfWork.SaveAsync();
            return article.Title;
        }
    }
}
