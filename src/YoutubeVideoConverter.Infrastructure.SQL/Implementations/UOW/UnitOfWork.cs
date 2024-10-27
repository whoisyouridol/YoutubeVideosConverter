using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions;
using YoutubeVideoConverter.Infrastructure.SQL.Persistance;

namespace YoutubeVideoConverter.Infrastructure.SQL.Implementations.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private Dictionary<Type, object> _repositories;
        private readonly IServiceProvider _serviceProvider;

        public UnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
        {
            _dbContext = context;
            _serviceProvider = serviceProvider;
            _repositories = new Dictionary<Type, object>();

            InitializeRepositories();
        }

        private void InitializeRepositories()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var repositoryTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Select(t => new
                {
                    ImplementationType = t,
                    InterfaceTypes = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseRepository<>))
                })
                .SelectMany(t => t.InterfaceTypes, (t, i) => new
                {
                    RepositoryType = t.ImplementationType,
                    InterfaceType = i
                })
                .ToList();

            foreach (var repoInfo in repositoryTypes)
            {
                var entityType = repoInfo.InterfaceType.GetGenericArguments()[0];

                var repositoryInstance = _serviceProvider.GetService(repoInfo.InterfaceType);

                if (repositoryInstance != null)
                    _repositories[entityType] = repositoryInstance;
            }
        }
        public IBaseRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
                return (IBaseRepository<T>)_repositories[typeof(T)];

            var repositoryInstance = new BaseRepository<T>(_dbContext);
            _repositories.Add(typeof(T), repositoryInstance);
            return repositoryInstance;
        }
        public TRepository GetRepository<TRepository, TEntity>()
         where TRepository : IBaseRepository<TEntity>
         where TEntity : class
        {
            var repositoryInstance = _serviceProvider.GetService<TRepository>();

            if (repositoryInstance != null)
                return repositoryInstance;

            throw new Exception($"Repository of type {typeof(TRepository).Name} not found.");
        }
        public void CreateTransaction() => _dbContext.Database.BeginTransaction();

        public void Commit() => _dbContext.Database.CommitTransaction();

        public void Rollback() => _dbContext.Database.RollbackTransaction();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _dbContext.SaveChangesAsync(cancellationToken);


    }
}
