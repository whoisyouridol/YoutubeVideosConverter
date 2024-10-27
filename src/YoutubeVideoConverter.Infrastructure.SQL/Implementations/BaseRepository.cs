using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions;
using YoutubeVideoConverter.Infrastructure.SQL.Persistance;

namespace YoutubeVideoConverter.Infrastructure.SQL.Implementations
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> AddAsync(T entity)
        {
            var result = await _context.Set<T>().AddAsync(entity);
            return result.Entity;
        }

        public async Task AddRangeAsync(ICollection<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public T Update(T entity)
        {
            var result = _context.Set<T>().Update(entity);
            return result.Entity;
        }

        public void UpdateRange(ICollection<T> entities)
        {
            _context.Set<T>().UpdateRange(entities);
        }
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(ICollection<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }
        /// <summary>
        /// Immediately deletes entities from database, without SaveChanges()
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task ExecuteDeleteAsync(Expression<Func<T, bool>> predicate)
        {
            await _context.Set<T>().Where(predicate).ExecuteDeleteAsync();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, bool trackChanges = false)
        {
            if (trackChanges)
                return await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);

            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> select, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().AsNoTracking().Where(predicate).Select(select).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> sortPredicate, bool isDescending, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = _context.Set<T>().AsNoTracking().AsQueryable();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            if (sortPredicate != null)
                query = isDescending ? query.OrderByDescending(sortPredicate) : query.OrderBy(sortPredicate);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = _context.Set<T>().AsNoTracking();

            foreach (var property in includeProperties)
                query = query.Include(property);

            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<T> FindAsync(object identifier, CancellationToken cancellationToken)
        {
            var a = _context.Set<T>().AsNoTracking();
            return await _context.Set<T>().FindAsync(identifier, cancellationToken);
        }
        public IQueryable<IGrouping<TKey, TSource>> GroupBy<TKey, TSource>(Expression<Func<TSource, TKey>> keySelector) where TSource : class
            => _context.Set<TSource>().GroupBy(keySelector);
        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate, bool trackChanges = false)
        {
            if (trackChanges)
                return _context.Set<T>().Where(predicate);

            return _context.Set<T>().AsNoTracking().Where(predicate);
        }

        public IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query;
            if (trackChanges)
                query = _context.Set<T>();
            else
                query = _context.Set<T>().AsNoTracking();

            foreach (var property in includeProperties)
                query = query.Include(property);

            return query.Where(predicate);
        }

        public IQueryable<TResult> GetWhere<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> select, bool trackChanges = false)
        {
            if (trackChanges)
                return _context.Set<T>().Where(predicate).Select(select);

            return _context.Set<T>().AsNoTracking().Where(predicate).Select(select);
        }
        public IQueryable<TResult> GetWhere<TResult>(Expression<Func<T, TResult>> select, bool trackChanges = false)
        {
            if (trackChanges)
                return _context.Set<T>().Select(select);

            return _context.Set<T>().Select(select);
        }
        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<T>().CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().AnyAsync(predicate, cancellationToken);
        }
    }
}
