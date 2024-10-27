using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeVideoConverter.Infrastructure.SQL.Abstractions
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(ICollection<T> entities);
        T Update(T entity);
        void UpdateRange(ICollection<T> entities);
        void Remove(T entity);
        void RemoveRange(ICollection<T> entities);

        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, bool trackChanges = false);
        Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> select, CancellationToken cancellationToken);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> sortPredicate, bool isDescending, params Expression<Func<T, object>>[] includeProperties);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<T> FindAsync(object identifier, CancellationToken cancellationToken);
        IQueryable<IGrouping<TKey, TSource>> GroupBy<TKey, TSource>(Expression<Func<TSource, TKey>> keySelector) where TSource : class;
        IQueryable<T> GetAll();
        IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<TResult> GetWhere<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> select, bool trackChanges = false);
        IQueryable<TResult> GetWhere<TResult>(Expression<Func<T, TResult>> select, bool trackChanges = false);
        Task<int> CountAsync(CancellationToken cancellationToken);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
        Task ExecuteDeleteAsync(Expression<Func<T, bool>> predicate);
    }
}
