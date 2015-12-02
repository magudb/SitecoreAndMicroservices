using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Jobs;
using Sitecore.Publishing;
using Web.CM.Serializer;

namespace Web.CM.Events
{
    public class PublishEndEvent
    {
        private readonly ItemSerializer _itemConverter;

        public PublishEndEvent()
        {
            _itemConverter = new ItemSerializer();
        }

        public void OnPublish(object sender, EventArgs args)
        {
          
            var publisher = Event.ExtractParameter(args, 0) as Publisher;
            if (publisher == null)
            {
                Log.Info("Cancel Process: Publisher Data is Null.", this);
                return;
            }

            // if publishing is not on Web Target than do nothing
            if (string.IsNullOrEmpty(publisher.Options?.TargetDatabase?.Name))
            {
                Log.Info("Cancel Process: Publisher.Options Data is Null.", this);
                return;
            }

            if (publisher.Options.TargetDatabase.Name != "web")
            {
                Log.Info("Cancel Process: Publishing TargetDatabase :" + publisher.Options.TargetDatabase.Name, this);
                return;
            }

            
            var routingKey = publisher.Options.RootItem.Paths.FullPath.ToLower().Replace("/", ".");
            routingKey = routingKey.Substring(1, (routingKey.Length - 1));
            const string exchange = "sitecore_publish";

            //var factory = new ConnectionFactory {HostName = "localhost"};
            //using (var connection = factory.CreateConnection())
            //{
            //    using (var channel = connection.CreateModel())
            //    {
            //        channel.ExchangeDeclare(exchange, "topic");
                    
            //        if (publisher.Options.RootItem == null)
            //        {
            //            Log.Info("Cancel Process: Sending to " + exchange + " with key " + routingKey, this);
            //            return;
            //        }
            //        publisher.Options.RootItem.Fields.ReadAll();

            //        var payload = _itemConverter.SerializeItem(publisher.Options.RootItem, "default");

            //        var body = Encoding.UTF8.GetBytes(payload);
            //        Log.Info("Sending to " + exchange + " with key " + routingKey, this);
            //        channel.BasicPublish(exchange, routingKey, null, body);
            //    }
            //}
        }
    }
}