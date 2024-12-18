using Domain.Entities;
using Domain.Interface;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Presistence.Repositories.BaseRepositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly BaseDBContext _dbContext;
        private readonly IMongoCollection<TEntity> _collection;

        public BaseRepository(BaseDBContext dbContext)
        {
            _dbContext = dbContext;
            _collection = _dbContext.Database.GetCollection<TEntity>(typeof(TEntity).Name);
        }

        public async Task AddAsync(TEntity entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public Task<IEnumerable<TEntity>> AllIncludingAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public Task Commit()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(ObjectId entityId)
        {
            await _collection.DeleteOneAsync(x=>x.Id == entityId);
        }

        public Task DeleteWhere(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TEntity>> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await _collection.Find(predicate).ToListAsync();
            return entities;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var entities = await _collection.Find(_ => true).ToListAsync();
            return entities;
        }

        public Task<TEntity> GetSingle(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public async Task<TEntity> GetSingleAsync(ObjectId id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            var filter = Builders<TEntity>.Filter.Eq(x=>x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity);

        }
    }
}
