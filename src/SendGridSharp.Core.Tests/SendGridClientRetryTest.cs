using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGridSharp.Core;
using Xunit;

namespace SendGridSharp.Core.Tests
{
    public class SendGridClientRetryTest
    {
        public IConfigurationRoot Configuration { get; set; }

        public SendGridClientRetryTest()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(AppContext.BaseDirectory)
                 .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        [Fact]
        public async Task RetryOverByDefaultPolicy()
        {
            var sw = new Stopwatch();

            sw.Restart();
            try
            {
                await SendAsync(
                    new SendGridClient(
                        new NetworkCredential(Configuration.GetSection("SendGridApiUser").Value, "dummy"),
                        new LoggerFactory(),
                        new SendGridRetryPolicy()));
                // 0, 3, 5 -> low:8
                // 0, 3, 5, 12 -> high:20
            }
            catch (SendGridException e)
            {
                Assert.InRange(sw.ElapsedMilliseconds / 1000, 8, 19);
            }
        }

        [Fact]
        public async Task RetryOverByCustomPolicyChangeParams()
        {
            var sw = new Stopwatch();
            sw.Restart();
            try
            {
                // change params.
                await SendAsync(
                    new SendGridClient(
                        new NetworkCredential(Configuration.GetSection("SendGridApiUser").Value, "dummy"),
                        new LoggerFactory(),
                        new SendGridRetryPolicy(2, TimeSpan.FromSeconds(6), 3.0)));
                // 0, 6 -> low:6
                // 0, 6, 9 -> high:18
            }
            catch (SendGridException e)
            {
                Assert.InRange(sw.ElapsedMilliseconds / 1000, 6, 17);
            }
        }

        [Fact]
        public async Task RetryOverByCustomPolicyIsTransient()
        {
            var sw = new Stopwatch();
            sw.Restart();
            try
            {
                // set IsTransient always false -> no Retrying.
                await SendAsync(
                    new SendGridClient(
                        new NetworkCredential(Configuration.GetSection("SendGridApiUser").Value, "dummy"),
                        new LoggerFactory(),
                        new SendGridRetryPolicy(3, TimeSpan.FromSeconds(10), 2.0))
                {
                    IsTransient = (ex) => false,
                    });
                // 0, 10, 18 -> low:28
                // 0, 10, 18, 28 -> high:56
            }
            catch (SendGridException e)
            {
                Assert.InRange(sw.ElapsedMilliseconds / 1000, 0, 9);
            }
        }


        private async Task SendAsync(SendGridClient client)
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Header.AddSubstitution("-name-", "Replaced Name");
            message.UseFooter("html", "text");

            message.Subject = "-name- さんへ";
            message.Text = "-name- さん";
            message.Html = "<p>-name- さん</p>";

            await client.SendAsync(message);
        }

    }
}
