using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.EntityFrameworkCore;

namespace WebsocketChat.Server.Helpers
{
    public static class PaginationHelper<T>
        where T : class
    {
        public const int DefaultId = -1;

        private const int ExtraPage = 1;
        private const int NoExtraPage = 0;

        public static async Task<List<T>> GetPageItems(IQueryable<T> itemsQuery, int? pageNumber, int? pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber ??= Library.Constants.MinPageNumber;
            pageSize ??= Library.Constants.DefaultPageSize;

            if (pageSize.Value < Library.Constants.MinPageSize)
            {
                throw new InvalidOperationException($"Page size value should be greater or equal {Library.Constants.MinPageSize}.");
            }

            var itemsCount = await itemsQuery.CountAsync(cancellationToken);
            var skipCount = (pageNumber.Value - 1) * pageSize.Value;

            if (skipCount >= itemsCount)
            {
                return Enumerable.Empty<T>().ToList();
            }

            // for 7 items per page:
            // 35 items = 5 pages, 37 items = 6 pages
            var lastPageNumber = itemsCount / pageSize.Value +
                (itemsCount % pageSize.Value == 0 ? NoExtraPage : ExtraPage);

            if (pageNumber < Library.Constants.MinPageNumber || pageNumber.Value > lastPageNumber)
            {
                throw new InvalidOperationException($"Page number value should be between {Library.Constants.MinPageNumber} and {lastPageNumber} for size of {pageSize.Value} items per page.");
            }

            return await itemsQuery
                .Skip(skipCount)
                .Take(pageSize.Value)
                .ToListAsync(cancellationToken);
        }

        public static async Task<int> GetTotalPagesCountAsync<T>(IQueryable<T> itemsQuery, int? pageSize,
            CancellationToken cancellationToken = default)
        {
            pageSize ??= Library.Constants.DefaultPageSize;

            if (pageSize.Value < Library.Constants.MinPageSize)
            {
                throw new InvalidOperationException($"Page size must be at least {Library.Constants.MinPageSize}.");
            }

            var itemsCount = await itemsQuery.CountAsync(cancellationToken);

            var totalPages = itemsCount / pageSize.Value +
                             (itemsCount % pageSize.Value > 0 ? ExtraPage : NoExtraPage);

            return totalPages;
        }
    }
}
