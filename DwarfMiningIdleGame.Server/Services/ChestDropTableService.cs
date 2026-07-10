using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwarfMiningIdleGame.Server.Entities.GameEntities;
using Microsoft.EntityFrameworkCore;

namespace DwarfMiningIdleGame.Server.Services
{
    public interface IChestDropTableService
    {
        Task<ChestDropTable> GetChestDropTableByLevelAsync(int level);
        Task<ChestDropTable> CreateChestDropTableAsync(int level, IEnumerable<DropEntry> entries);
        Task<IEnumerable<ChestDropTable>> GetAllChestDropTablesAsync();
        Task DeleteChestDropTableAsync(Guid chestDropTableId);
    }

    public class ChestDropTableService : IChestDropTableService
    {
        private readonly GameDbContext _dbContext;

        public ChestDropTableService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Получить таблицу дропа для определённого уровня сундука
        /// </summary>
        public async Task<ChestDropTable> GetChestDropTableByLevelAsync(int level)
        {
            return await _dbContext.ChestDropTables
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChestLevel == level);
        }

        /// <summary>
        /// Создать новую таблицу дропа с записями
        /// </summary>
        public async Task<ChestDropTable> CreateChestDropTableAsync(int level, IEnumerable<DropEntry> entries)
        {
            var chestDropTable = new ChestDropTable
            {
                Id = Guid.NewGuid(),
                ChestLevel = level,
                Entries = new List<DropEntry>(entries)
            };

            _dbContext.ChestDropTables.Add(chestDropTable);
            await _dbContext.SaveChangesAsync();

            return chestDropTable;
        }

        /// <summary>
        /// Получить все таблицы дропа
        /// </summary>
        public async Task<IEnumerable<ChestDropTable>> GetAllChestDropTablesAsync()
        {
            return await _dbContext.ChestDropTables
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Удалить таблицу дропа по ID (каскадное удаление записей)
        /// </summary>
        public async Task DeleteChestDropTableAsync(Guid chestDropTableId)
        {
            var chestDropTable = await _dbContext.ChestDropTables
                .FirstOrDefaultAsync(c => c.Id == chestDropTableId);

            if (chestDropTable != null)
            {
                _dbContext.ChestDropTables.Remove(chestDropTable);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

