SendGridSharp.Core
=============

Yet Another SendGrid Client for .NET Core
## Updates

### 1.1.1
* reuse HttpClient.
  * and Refleshing at firing Exceptions.
* Refactoring.

### 1.1.0
* Add Logging.
* Support Basic Retry Policy at SendAsync() by default. 
* RetryPolicy customization.

Increase intervals calculated by this formula `TimeSpan = backoff * currentTimes ^ 2 + InitialDelay`. 
```
await SendAsync(
  new SendGridClient(
    new NetworkCredential("account", "password"),
    new LoggerFactory(),
    new SendGridRetryPolicy(5, TimeSpan.FromSeconds(5), 3.0)));
```
If you set this, it means that ,max retry = 5 times, fist waiting = 5 sec,  backoff = 3.0 => { 0,-> 8,->, 17,-> 32,-> 53 }.

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
