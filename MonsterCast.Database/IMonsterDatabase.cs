using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCast.Database
{
    public interface IMonsterDatabase
    {
        SQLiteAsyncConnection Database { get; }
        Task<int> ConnectAsync();
        Task<int> CloseAsync();
        Task<bool> IsAlreadyExist<T>(Expression<Func<T, bool>> predicate) 
            where T: class, new();

        Task<int> AddAsync<T>(T item);
        Task<int> RemoveAsync<T>(T item);

        EventHandler<DatabaseUpdateEventArgs> DatabaseUpdated { get; set; }
    }
}
