using Microsoft.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RestApi.Middleware
{
    public class Request_Response_Middleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        public Request_Response_Middleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory
                      .CreateLogger<Request_Response_Middleware>();
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }
        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
            await LogResponse(context);
        }
        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            _logger.LogInformation($"\n" +
                                   $"\n      Http Request Information:\n" +
                                   $"      Schema:{context.Request.Scheme}\n" +
                                   $"      Host: {context.Request.Host}\n" +
                                   $"      Path: {context.Request.Path}\n" +
                                   $"      Request Body: {ReadStreamInChunks(requestStream)}\n");
            context.Request.Body.Position = 0;
        }
        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;
            var watch = new Stopwatch();
            watch.Start();
            await _next(context);
            watch.Stop();
            _logger.LogInformation($"Response Time: {watch.ElapsedMilliseconds}");
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var data = JsonConvert.SerializeObject(text);
            _logger.LogInformation($"\n" +
                                   $"      Http Response Information:\n" +
                                   $"      StatusCode: {context.Response.StatusCode}\n" +
                                   $"      ContentType: {context.Response.ContentType}\n" +
                                   $"      Response Body: \n{data}\n");
            await responseBody.CopyToAsync(originalBodyStream);
        }
        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Request_Response_Middleware>();
        }
    }
}