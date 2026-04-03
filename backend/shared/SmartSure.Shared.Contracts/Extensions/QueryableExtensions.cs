using Microsoft.EntityFrameworkCore;
using SmartSure.Shared.Contracts.DTOs;

namespace SmartSure.Shared.Contracts.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, 
        int page, 
        int pageSize)
    {
        var result = new PagedResult<T>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = await query.CountAsync()
        };

        var pageCount = (double)result.TotalCount / pageSize;
        result.Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return result;
    }
}
