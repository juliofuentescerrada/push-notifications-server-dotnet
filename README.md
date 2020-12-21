![CI](https://github.com/juliofuentescerrada/push-notifications-server-dotnet/workflows/CI/badge.svg)
[![NuGet](https://img.shields.io/nuget/vpre/Pusher.PushNotifications.svg)](https://www.nuget.org/packages/Pusher.PushNotifications)

# push-notifications-server-dotnet 

Unofficial Pusher Beams .NET Server SDK

## Installation
The Beams .NET server SDK is available on nuget [here](https://www.nuget.org/packages/Pusher.PushNotifications).

You can install this SDK by using [NuGet](https://www.nuget.org/):
```dotnetcli
Install-Package Pusher.PushNotifications
```
Or via the .NET Core command line interface:
```dotnetcli
dotnet add package Pusher.PushNotifications
```
## Usage
### Configuring the SDK for Your Instance
Use your instance id and secret (you can get these from the [dashboard](https://dash.pusher.com/beams)) to create a Beams PushNotifications instance:

```C#
var pushNotificationsOptions = new PushNotificationsOptions
{
    InstanceId = "YOUR_INSTANCE_ID_HERE",
    SecretKey = "YOUR_SECRET_KEY_HERE"
};

var pushNotifications = new PushNotifications(new HttpClient(), pushNotificationsOptions);
```
### Publishing a Notification
Once you have created your PushNotifications instance you can publish a push notification to your registered & subscribed devices:
```C#
var interests = new List<string> { "donuts", "pizza" };

var notification = new Dictionary<string, object>
{
    { "title", "hello" },
    { "body", "Hello world" }
};

var alert = new Dictionary<string, object>
{
    { "title", "hello" },
    { "body", "Hello world" }
};

var publishRequest = new Dictionary<string, object>
{
    { "fcm",  new { notification } },
    { "apns",  new { aps = new { alert } }}
};

pushNotifications.PublishToInterests(interests, publishRequest);
```
