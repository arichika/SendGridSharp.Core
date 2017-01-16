using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGridSharp.Core;
using Xunit;

namespace SendGridSharp.Core.Tests
{
    public class SendGridClientSendTest
    {
        public IConfigurationRoot Configuration { get; set; }

        private SendGridClient Client { get; set; }

        public SendGridClientSendTest()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(AppContext.BaseDirectory)
                 .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            Client = new SendGridClient(
                new NetworkCredential(
                    Configuration.GetSection("SendGridApiUser").Value,
                    Configuration.GetSection("SendGridApiKey").Value));
        }

        [Fact]
        public void Send()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Header.AddSubstitution("-name-", "Replaced Name");
            message.UseFooter("html", "text");

            message.Subject = "-name- さんへ";
            message.Text = "-name- さん";
            message.Html = "<p>-name- さん</p>";

            Client.Send(message);
        }

        [Fact]
        public async Task SendAsync()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Header.AddSubstitution("-name-", "Replaced Name");
            message.UseFooter("html", "text");

            message.Subject = "-name- さんへ";
            message.Text = "-name- さん";
            message.Html = "<p>-name- さん</p>";

            await Client.SendAsync(message);
        }

        [Fact]
        public void Schedule()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Header.AddSubstitution("-name-", "Replaced Name");
            message.Header.AddSendAt(DateTimeOffset.Now.AddSeconds(30));

            message.Subject = "-name- さんへ";
            message.Text = "-name- さん";
            message.Html = "<p>-name- さん</p>";

            Client.Send(message);
        }

        [Fact]
        public void ScheduleMulti()
        {
            var sendAt = DateTimeOffset.Now.AddMinutes(15);

            for (int i = 0; i < 5; i++)
            {
                var message = new SendGridMessage();

                message.To.Add(Configuration.GetSection("MailTo").Value);
                message.From = Configuration.GetSection("MailFrom").Value;

                message.Header.AddSubstitution("-name-", "Replaced Name");
                message.Header.AddSendAt(sendAt);

                message.Subject = "-name- さんへ" + i;
                message.Text = "-name- さん";
                message.Html = "<p>-name- さん</p>";

                Client.Send(message);

                Thread.Sleep(200);
            }
        }

        [Fact]
        public void Attachment()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Subject = "file attachment";
            message.Text = "file attachment test";

            message.AddAttachment(Configuration.GetSection("AttachmentImage").Value);

            Client.Send(message);
        }

        [Fact]
        public void EmbedImage()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Subject = "file attachment";
            message.Html = "file attachment test<br /><img src=\"cid:hogehoge\" /><br />inline image";

            message.AddAttachment(Configuration.GetSection("AttachmentImage").Value, "Embed.jpg");
            message.Content.Add("Embed.jpg", "hogehoge");

            Client.Send(message);
        }

        [Fact]
        public void TemplateEngine()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Subject = " ";
            message.Text = " ";

            message.UseTemplateEngine("91ba5fd7-984c-4810-95fd-030be7242106");

            message.Header.AddSubstitution("-name-", "Replaced Name");
            message.Header.AddSubstitution("-url-", "http://example.com/");

            Client.Send(message);
        }

        [Fact]
        public void OpenTrack()
        {
            var message = new SendGridMessage();

            message.To.Add(Configuration.GetSection("MailTo").Value);
            message.From = Configuration.GetSection("MailFrom").Value;

            message.Header.AddSubstitution("-name-", "Replaced Name");
            message.UseFooter("html", "text");

            message.Subject = "-name- さんへ";
            message.Text = "-name- さん";
            message.Html = "<p>-name- さん</p>";

            message.UseOpenTrack();

            Client.Send(message);
        }
    }
}
