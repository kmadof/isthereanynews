﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Itan.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace Itan.Core
{
    public class GetAllChannelsViewModelsRequest : IRequest<List<ChannelViewModel>>
    {
    }

    public class GetAllChannelsViewModelsRequestHandler : IRequestHandler<GetAllChannelsViewModelsRequest, List<ChannelViewModel>>
    {
        private string connectionString;

        public GetAllChannelsViewModelsRequestHandler(ConnectionOptions options)
        {
            this.connectionString = options.SqlReader;
        }

        public async Task<List<ChannelViewModel>> Handle(
            GetAllChannelsViewModelsRequest _,
            CancellationToken __)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var sqlQuery =
                    "select c.id, c.title, c.description, c.Url, count(n.Id) as NewsCount from Channels c left join News n on n.ChannelId = c.Id group by c.Id,c.Title,c.Description, c.Url";
                var result = await connection.QueryAsync<ChannelViewModel>(sqlQuery);
                return result.ToList();
            }
        }
    }
}