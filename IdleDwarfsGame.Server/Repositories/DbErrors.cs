using System.Data.Common;
using Microsoft.Data.SqlClient;

// ExecuteUpdate/ExecuteDelete идут мимо SaveChanges и бросают исключение провайдера, а не DbUpdateException
public static class DbErrors
{
    public static bool IsUniqueViolation(DbException ex) => ex switch
    {
        // 2601 — уникальный индекс, 2627 — первичный ключ или UNIQUE-ограничение
        SqlException sql => sql.Number is 2601 or 2627,
        // In-memory SQLite в тестах: SQLITE_CONSTRAINT. Пакета SQLite у сервера нет — код читаем по имени свойства
        _ => ex.GetType().GetProperty("SqliteErrorCode")?.GetValue(ex) is 19
    };
}
