using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeBlog.Service.Services.Abstraction
{
    public interface IDashboardService
    {
         Task<List<int>> GetYearlyArticleCount();
        Task<int> GetTotalArticleCount();
        Task<int> GetTotalCategoryCount();
    }
}
