using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeVideoConverter.Infrastructure.SQL.Models;

namespace YoutubeVideoConverter.Infrastructure.SQL.Abstractions
{
    public interface IConvertLogRepository : IBaseRepository<ConvertLog>
    {
    }
}
