// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Simple REST service client
    /// </summary>
    public class SimpleServiceClient : IDisposable
    {
        private static readonly AssemblyName AssemblyName = new AssemblyName(
            typeof(SimpleServiceClient).GetTypeInfo().Assembly.FullName);

        private static readonly ProductInfoHeaderValue UserAgent = new ProductInfoHeaderValue(
            AssemblyName.Name, 
            AssemblyName.Version.ToString());

        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        private readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(
            () => CreateClient(DefaultTimeout),
            LazyThreadSafetyMode.PublicationOnly);

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        public TimeSpan Timeout
        {
            get { return _httpClient.Value.Timeout; }
            set { _httpClient.Value.Timeout = value; }
        }

        public class SimpleServiceResult<TResult, TErrorResult>
        {
            public TResult Result { get; set; }
            public TErrorResult ErrorResult { get; set; }
            public HttpStatusCode HttpStatusCode { get; set; }
        }

        public virtual void Dispose()
        {
            if (_httpClient.IsValueCreated)
            {
                _httpClient.Value.Dispose();
            }
        }

        /// <summary>
        /// Issue an HTTP GET request
        /// </summary>
        /// <typeparam name="TResult">The result data contract type</typeparam>
        /// <typeparam name="TErrorResult">The error result data contract type</typeparam>
        /// <param name="hostname">The HTTP host</param>
        /// <param name="relativeUri">A relative URL to append at the end of the HTTP host</param>
        /// <param name="cancellationToken"></param>
        /// <param name="requestParameters">Optional query string parameters</param>
        /// <param name="extraHeaders">Optional HTTP headers</param>
        public async Task<SimpleServiceResult<TResult, TErrorResult>> GetAsync<TResult, TErrorResult>(
            Uri hostname,
            string relativeUri,
            CancellationToken cancellationToken,
            IEnumerable<KeyValuePair<string, string>> requestParameters = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
            where TResult : class
            where TErrorResult : class
        {
            Uri uri = BuildUri(hostname, relativeUri, requestParameters);

            using (HttpRequestMessage httpRequestMessage = CreateHttpRequest(HttpMethod.Get, uri, null, extraHeaders))
            using (HttpResponseMessage httpResponseMessage = await _httpClient.Value.SendAsync(httpRequestMessage, cancellationToken))
            {
                return await ParseResponseAsync<TResult, TErrorResult>(httpResponseMessage);
            }
        }

        public async Task<TResult> GetAsync<TResult>(
            Uri hostname, 
            string relativeUri,
            CancellationToken cancellationToken,
            IEnumerable<KeyValuePair<string, string>> requestParameters = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
            where TResult : class
        {
            SimpleServiceResult<TResult, TResult> result = await GetAsync<TResult, TResult>(
                hostname, 
                relativeUri, 
                cancellationToken, 
                requestParameters, 
                extraHeaders);

            return result.Result ?? result.ErrorResult;
        }

        /// <summary>
        /// Issue an HTTP POST request
        /// </summary>
        /// <typeparam name="TResult">The result data contract type</typeparam>
        /// <typeparam name="TErrorResult">The error result data contract type</typeparam>
        /// <typeparam name="TRequest">The request data contract type</typeparam>
        /// <param name="hostname">The HTTP host</param>
        /// <param name="relativeUri">A relative URL to append at the end of the HTTP host</param>
        /// <param name="requestPayload"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="requestParameters">Optional query string parameters</param>
        /// <param name="extraHeaders">Optional HTTP headers</param>
        public async Task<SimpleServiceResult<TResult, TErrorResult>> PostAsync<TResult, TErrorResult, TRequest>(
            Uri hostname, 
            string relativeUri,
            TRequest requestPayload,
            CancellationToken cancellationToken,
            IEnumerable<KeyValuePair<string, string>> requestParameters = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
            where TResult : class
            where TErrorResult : class
        {
            Uri uri = BuildUri(hostname, relativeUri, requestParameters);

            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            using (HttpContent content = CreateHttpContent(requestPayload, writer, stream))
            using (HttpRequestMessage requestMessage = CreateHttpRequest(HttpMethod.Post, uri, content, extraHeaders))
            using (HttpResponseMessage result = await _httpClient.Value.SendAsync(requestMessage, cancellationToken))
            {
                return await ParseResponseAsync<TResult, TErrorResult>(result);
            }
        }

        public async Task<TResult> PostAsync<TResult, TRequest>(
            Uri hostname, string relativeUri,
            TRequest requestPayload,
            CancellationToken cancellationToken,
            IEnumerable<KeyValuePair<string, string>> requestParameters = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
            where TResult : class
        {
            SimpleServiceResult<TResult, TResult> result = await PostAsync<TResult, TResult, TRequest>(
                hostname, 
                relativeUri, 
                requestPayload, 
                cancellationToken, 
                requestParameters, 
                extraHeaders);

            return result.Result ?? result.ErrorResult;
        }

        private static Uri BuildUri(
            Uri hostname, 
            string relativeUri, 
            IEnumerable<KeyValuePair<string, string>> requestParameters)
        {
            string relUri = requestParameters == null
                ? relativeUri
                : requestParameters.Aggregate(relativeUri,
                    (current, param) => current + (current.Contains("?") ? "&" : "?") + param.Key + "=" + param.Value);

            return new Uri(hostname, relUri);
        }

        private static HttpClient CreateClient(
            TimeSpan timeout, 
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.UserAgent.Add(UserAgent);
            client.Timeout = timeout;

            if (extraHeaders != null)
            {
                foreach (var header in extraHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            return client;
        }

        protected virtual HttpContent CreateHttpContent<TRequest>(
            TRequest requestPayload, 
            StreamWriter writer,
            MemoryStream stream)
        {
            _jsonSerializer.Serialize(writer, requestPayload, typeof(TRequest));
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            HttpContent content = new StreamContent(stream);
            content.Headers.Add("Content-Type", "application/json");
            return content;
        }

        private static HttpRequestMessage CreateHttpRequest(
            HttpMethod method, 
            Uri uri, 
            HttpContent content,
            IEnumerable<KeyValuePair<string, string>> extraHeaders)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, uri);
            if (content != null)
            {
                message.Content = content;
            }

            if (extraHeaders != null)
            {
                foreach (var header in extraHeaders)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            return message;
        }

        private async Task<SimpleServiceResult<TResult, TErrorResult>> ParseResponseAsync<TResult, TErrorResult>(HttpResponseMessage message)
            where TResult : class
            where TErrorResult : class
        {
            if (message.Content == null)
            {
                return new SimpleServiceResult<TResult, TErrorResult>
                {
                    HttpStatusCode = message.StatusCode
                };
            }

            using (Stream stream = await message.Content.ReadAsStreamAsync())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return new SimpleServiceResult<TResult, TErrorResult>
                    {
                        Result =
                            message.IsSuccessStatusCode
                                ? _jsonSerializer.Deserialize(reader, typeof(TResult)) as TResult
                                : null,
                        ErrorResult =
                            message.IsSuccessStatusCode
                                ? null
                                : _jsonSerializer.Deserialize(reader, typeof(TErrorResult)) as TErrorResult,
                        HttpStatusCode = message.StatusCode
                    };
                }
            }
        }
    }
}
