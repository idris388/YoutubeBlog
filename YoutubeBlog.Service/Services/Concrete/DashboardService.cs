using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeBlog.Data.UnitOfWorks;
using YoutubeBlog.Entity.Entities;
using YoutubeBlog.Service.Services.Abstraction;

namespace YoutubeBlog.Service.Services.Concrete
{
    public class DashboardService:IDashboardService
    {
        private readonly IUnitOfWork unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<List<int>> GetYearlyArticleCount()
        {
            var articles=await unitOfWork.GetRepository<Article>().GetAllAsync(x=>!x.IsDeleted);
            var startDate = DateTime.Now.Date;
            startDate = new DateTime(startDate.Year, 1, 1);
            List<int> datas = new();
            for (int i = 0; i <= 12; i++)
            {
                var startedDate = new DateTime(startDate.Year, i, 1);
                var endedDate = startDate.AddMonths(1);
                var data = articles.Where(x => x.CreatedDate > startedDate && x.CreatedDate < endedDate).Count();
                datas.Add(data);
            }
            return datas;
        }
        public async Task<int> GetTotalArticleCount()
        {
            var articles = await unitOfWork.GetRepository<Article>().CountAsync();
            return articles;
        }

        public async Task<int> GetTotalCategoryCount()
        {
            var categoryCount = await unitOfWork.GetRepository<Category>().CountAsync();
            return categoryCount;
        }
    }
}
