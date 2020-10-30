using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MyCourse.Models.Services.Infrastructure
{
    public interface IDatabaseAccessor
    {
        //Task<DataSet> QueryAsync(FormattableString query);
        //Task<T> QueryScalarAsync<T>(FormattableString formattableQuery);
        //Task<int> CommandAsync(FormattableString formattableCommand);
        Task<DataSet> QueryAsync(FormattableString formattableQuery, CancellationToken token = default(CancellationToken));
        Task<T> QueryScalarAsync<T>(FormattableString formattableQuery, CancellationToken token = default(CancellationToken));
        Task<int> CommandAsync(FormattableString formattableCommand, CancellationToken token = default(CancellationToken));

    }
}