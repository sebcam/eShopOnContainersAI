﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Infrastructure.ActionResults;
using Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Infrastructure.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment env;
        private readonly ILogger<HttpGlobalExceptionFilter> logger;

        public HttpGlobalExceptionFilter(IHostingEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            if (context.Exception.GetType() == typeof(ArtificialIntelligenceDomainException))
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { context.Exception.Message }
                };

                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error ocurr.Try it again." }
                };

                if (env.IsDevelopment())
                {
                    json.DeveloperMeesage = context.Exception;
                }

                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            context.ExceptionHandled = true;
        }

        private class JsonErrorResponse
        {
            public string[] Messages { get; set; }

            public object DeveloperMeesage { get; set; }
        }
    }
}