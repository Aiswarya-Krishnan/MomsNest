using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.DataAccess.Repository
{
    public class Repository<T>: IRepository<T> where T:class
    {
        private readonly AppDbContext context;
        internal DbSet<T> dbset;
        public Repository(AppDbContext context)
        {
            this.context = context;
            this.dbset=context.Set<T>();
        }

        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = dbset;
            query= query.Where(filter);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = dbset;
           
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity);
        }
    }


}
