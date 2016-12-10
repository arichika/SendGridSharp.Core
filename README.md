SendGridSharp.Core
=============

Yet Another SendGrid Client for .NET Core

## How To Install

Please Install by NuGet. Just only.

```
PM> Install-Package SendGridSharp.Core
```

## Usage

```csharp
// Use API Key
var client = new SendGridClient("API_KEY");
// use SendGrid credential
//var client = new SendGridClient(new NetworkCredential("USERNAME", "PASSWORD"));

var message = new SendGridMessage();

message.To.Add("****@example.com");
message.From = "****@example.com";

message.Header.AddSubstitution("-name-", "customer");

message.Subject = "Dear -name- ";
message.Html = "<p>html message</p>";
message

client.Send(message);
```
