﻿using System.Net;
using System.Threading.Tasks;
using Itan.Core.Handlers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Itan.Api.Controllers
{
    public class ItanExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (ItanValidationException e)
            {
                var status = HttpStatusCode.BadRequest;
                var resultToSerialize = new {e.Message, e.StackTrace};
                var result = JsonConvert.SerializeObject(resultToSerialize);
                context.Response.StatusCode = (int) status;
                await context.Response.WriteAsync(result);
            }
        }
    }
}