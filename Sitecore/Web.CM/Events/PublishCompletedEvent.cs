using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Web.CM.Serializer;

namespace Web.CM.Events
{
    public class PublishCompletedEvent
    {
        private const string Exchange = "content_publish";
        private readonly ItemSerializer _itemConverter;
        private readonly ConnectionFactory _factory;
        private Database _db;

        public PublishCompletedEvent()
        {
            _itemConverter = new ItemSerializer();
            _factory = new ConnectionFactory {HostName = "localhost"};
            _db = Sitecore.Data.Database.GetDatabase("master");
        }

        public void OnPublish(object sender, EventArgs args)
        {
            var sitecoreArgs = args as SitecoreEventArgs;


            if (sitecoreArgs == null)
            {
                Log.Info("Cancel Process: sitecoreArgs  is Null.", this);
                return;
            }

            var publishingOptions = sitecoreArgs.Parameters[0] as IEnumerable<DistributedPublishOptions>;

            // if publishing is not on Web Target than do nothing
            if (publishingOptions == null)
            {
                Log.Info("Cancel Process: PublishingOptions Data is Null.", this);
                return;
            }
          
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    foreach (var options in publishingOptions)
                    {


                        if (options.TargetDatabaseName != "web")
                        {
                            Log.Info(
                                "Cancel Process: Publishing TargetDatabase :" + options.TargetDatabaseName,
                                this);
                            return;
                        }

                        var item = _db.GetItem(new ID(options.RootItemId), Language.Parse(options.LanguageName));

                        var routingKey = item.Paths.FullPath.ToLower().Replace("/", ".");
                        routingKey = routingKey.Substring(1, (routingKey.Length - 1));

                        channel.ExchangeDeclare(Exchange, "topic");

                        item.Fields.ReadAll();

                        var payload = _itemConverter.SerializeItem(item, "default");

                        var body = Encoding.UTF8.GetBytes(payload);
                        Log.Info("Sending to " + Exchange + " with key " + routingKey, this);
                        channel.BasicPublish(Exchange, routingKey, null, body);
                    }
                }
            }
        }
    }
}