using Domain.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        #region Methods

        /// <summary>
        /// get all using query
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns> IEnumerable of type TEntity </returns>
        Task<IEnumerable<TEntity>> AllIncludingAsync(params Expression<Func<TEntity, object>>[] includeProperties);

        /// <summary>
        /// get all 
        /// </summary>
        /// <returns> IEnumerable of type TEntity </returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Count
        /// </summary>
        /// <returns> int </returns>
        Task<int> CountAllAsync();

        /// <summary>
        /// get single entity by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns> single entity of type TEntity </returns>
        Task<TEntity> GetSingleAsync(ObjectId id);

        /// <summary>
        /// get single entity by query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns> single entity of type TEntity </returns>
        Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// get single entity by query
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="includeProperties"></param>
        /// <returns> single entity of type TEntity </returns>
        Task<TEntity> GetSingle(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        /// <summary>
        /// find entities by query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns> IEnumerable of type TEntity </returns>
        Task<IEnumerable<TEntity>> FindBy(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// add entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> void</returns>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> void</returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> void</returns>
        Task DeleteAsync(ObjectId entityId);

        /// <summary>
        /// Delete entity by query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns> void</returns>
        Task DeleteWhere(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Commit
        /// </summary>
        /// <returns> void</returns>
        Task Commit();
        #endregion
    }

}
