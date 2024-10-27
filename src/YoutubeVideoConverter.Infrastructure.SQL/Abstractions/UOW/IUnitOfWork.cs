using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW
{
    public interface IUnitOfWork
    {
        public IBaseRepository<T> GetRepository<T>() where T : class;
        TRepository GetRepository<TRepository, TEntity>() where TEntity : class where TRepository : IBaseRepository<TEntity>;
        void CreateTransaction();
        void Commit();
        void Rollback();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}