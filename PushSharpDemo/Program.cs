using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushSharpDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter to push notification...");
            Console.ReadKey();

            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "cert", "password");
            var apnsBroker = new ApnsServiceBroker(config);

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException			
                        Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                Console.WriteLine("Apple Notification Sent!");
            };


            apnsBroker.Start();

            // Queue a notification to send
            apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = "device-token-should-be-received-from-mobile-device",
                Payload = JObject.Parse("{\"aps\":{\"badge\":7}}")
            }); 

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            apnsBroker.Stop();

            Console.ReadKey();
        }
    }
}
