using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.DataLayer;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;


namespace TicTacToe.Repository
{
    public class DBRepository<T> : IRepository<T> where T : class
    {
        protected DbContext Context;
        protected DbSet<T> Entities;
      
        public DBRepository():this(new DataContext())
        {

        }
        public DBRepository(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentException("An instance of DbContext is required to use this repository.", "context");
            }
            this.Context = context;
            this.Entities = this.Context.Set<T>();
        }

        public virtual IQueryable<T> GetAll()
        {
            return this.Entities.AsQueryable();
        }

        public virtual T GetById(int id)
        {
            return this.Entities.Find(id);
        }

        public virtual void Add(T entity)
        {
            DbEntityEntry entry = this.Context.Entry(entity);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                this.Entities.Add(entity);
            }
        }

        public virtual void Update(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            DbEntityEntry entry = this.Context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                this.Entities.Attach(entity);
            }
            entry.State = EntityState.Modified;
        }

        public virtual void Remove(IEnumerable<T> entities)
        {
            foreach (var enitty in entities)
            {
                this.Delete(enitty);
            }          
        }
        public virtual void Delete(T entity)
        {
            DbEntityEntry entry = this.Context.Entry(entity);
            if (entry.State != EntityState.Deleted)
            {
                entry.State = EntityState.Deleted;
            }
            else
            {
                this.Entities.Attach(entity);
                this.Entities.Remove(entity);
            }
        }

        public virtual void Delete(int id)
        {

            var entity = this.GetById(id);
            if (entity != null)
            {
                this.Delete(entity);
            }
        }

        public virtual void Detach(T entity)
        {
            DbEntityEntry entry = this.Context.Entry(entity);

            entry.State = EntityState.Detached;
        }

       
    }
}
