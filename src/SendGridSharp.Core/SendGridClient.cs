﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SendGridSharp.Core
{
    public class SendGridClient
    {
        private readonly ILogger _logger = ApplicationLogging.LoggerFactory.CreateLogger<SendGridClient>();

        private const string Endpoint = "https://api.sendgrid.com/api/mail.send.json";
        private readonly string _apiKey;
        private readonly NetworkCredential _credentials;

        public readonly SendGridRetryPolicy SendGridRetryPolicy = new SendGridRetryPolicy();

        public Predicate<Exception> IsTransient = ex => ex is HttpRequestException || ex is SendGridException;


        public SendGridClient(NetworkCredential credentials)
        {
            _credentials = credentials;
        }

        public SendGridClient(NetworkCredential credentials,ILoggerFactory loggerFactory, SendGridRetryPolicy sendGridRetryPolicy = null)
        {
            _credentials = credentials;
            if (loggerFactory != null) _logger = loggerFactory.CreateLogger<SendGridClient>();
            SendGridRetryPolicy = sendGridRetryPolicy ?? new SendGridRetryPolicy();
        }

        public SendGridClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public SendGridClient(string apiKey, ILoggerFactory loggerFactory, SendGridRetryPolicy sendGridRetryPolicy = null)
        {
            _apiKey = apiKey;
            if (loggerFactory != null) _logger = loggerFactory.CreateLogger<SendGridClient>();
            SendGridRetryPolicy = sendGridRetryPolicy ?? new SendGridRetryPolicy();
        }


        public void Send(SendGridMessage message)
        {
            var content = GetContent(message);

            var client = new HttpClient(new WebApiHandler(_apiKey));

            var response = client.PostAsync(Endpoint, content).Result;

            var responseContent = response.Content.ReadAsStringAsync().Result;

            var result = JsonConvert.DeserializeObject<GenericResult>(responseContent);

            if (!result.IsSuccess)
            {
                throw new SendGridException(result.Errors[0]);
            }
        }

        private async Task SendAsyncInternal(SendGridMessage message)
        {
            var content = GetContent(message);

            var client = new HttpClient(new WebApiHandler(_apiKey));

            var response = await client.PostAsync(Endpoint, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<GenericResult>(responseContent);

            if (!result.IsSuccess)
            {
                throw new SendGridException(result.Message);
            }
        }

        public async Task SendAsync(SendGridMessage message)
        {
            var currentRetry = 0;
            for (;;)
            {
                try
                {
                    await SendAsyncInternal(message);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{ex}");
                    currentRetry++;

                    if (SendGridRetryPolicy.IsRetryOver(currentRetry) || !IsTransient(ex))
                        throw;
                }
                await Task.Delay(SendGridRetryPolicy.CalcWaitTimeSpan(currentRetry));
            }
        }

        private MultipartFormDataContent GetContent(SendGridMessage message)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(message.Headers)), "headers" },
                { new StringContent(message.From), "from" },
                { new StringContent(message.Subject), "subject" },
                { new StringContent(message.Header.ToString()), "x-smtpapi" }
            };

            if (_credentials != null)
            {
                content.Add(new StringContent(_credentials.UserName), "api_user");
                content.Add(new StringContent(_credentials.Password), "api_key");
            }

            if (message.FromName != null)
            {
                content.Add(new StringContent(message.FromName), "fromname");
            }

            if (message.ReplyTo != null)
            {
                content.Add(new StringContent(message.ReplyTo), "replyto");
            }

            if (message.Text != null)
            {
                content.Add(new StringContent(message.Text), "text");
            }

            if (message.Html != null)
            {
                content.Add(new StringContent(message.Html), "html");
            }

            if (message.Date.HasValue)
            {
                content.Add(new StringContent(message.Date.Value.ToString("r")), "date");
            }

            foreach (var to in message.To)
            {
                content.Add(new StringContent(to), "to[]");
            }

            foreach (var bcc in message.Bcc)
            {
                content.Add(new StringContent(bcc), "bcc[]");
            }

            foreach (var cc in message.Cc)
            {
                content.Add(new StringContent(cc), "cc[]");
            }

            foreach (var item in message.Files)
            {
                content.Add(new StreamContent(item.Value), "files[" + item.Key + "]");
            }

            foreach (var item in message.Content)
            {
                content.Add(new StringContent(item.Value), "content[" + item.Key + "]");
            }

            return content;
        }

        internal class WebApiHandler : DelegatingHandler
        {
            internal WebApiHandler(string apiKey)
                : base(new HttpClientHandler())
            {
                _apiKey = apiKey;
            }

            private readonly string _apiKey;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (_apiKey != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                }

                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
