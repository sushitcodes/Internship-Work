using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Form.Persistence;

public static class DbUpdateExceptionExtensions
{
    // 2601 = duplicate key in a unique index, 2627 = unique constraint violation
    public static bool IsUniqueViolation(this DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };

    // 547 = foreign key violation (for example a class id that does not exist)
    public static bool IsForeignKeyViolation(this DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 547 };
}