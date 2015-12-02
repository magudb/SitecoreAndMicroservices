using System.Text;
using RabbitMQ.Client;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.ItemProvider.SaveItem;
using Web.CM.Serializer;
using Debug = System.Diagnostics.Debug;

namespace Web.CM.PipelineBasedItemProviders
{
    public class SaveItemProcessor : Sitecore.Pipelines.ItemProvider.SaveItem.SaveItemProcessor
    {
        private const string Exchange = "content_publish";
        private readonly ConnectionFactory _factory;
        private readonly ItemSerializer _itemConverter;

        public SaveItemProcessor()
        {
            _itemConverter = new ItemSerializer();
            _factory = new ConnectionFactory {HostName = "localhost"};
        }

        public override void Process(SaveItemArgs args)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var item = args.Item;

                    if (!item.Database.Name.Equals("web") || !item.Paths.IsContentItem)
                    {
                        Log.Info(
                            "Cancel Process: Publishing TargetDatabase :" + item.Database.Name,
                            this);
                        return;
                    }


                    var routingKey = "add" + item.Paths.FullPath.ToLower().Replace("/", ".");
                   

                    channel.ExchangeDeclare(Exchange, "topic");

                    item.Fields.ReadAll();

                    var payload = _itemConverter.SerializeItem(item, "default");

                    var body = Encoding.UTF8.GetBytes(payload);
                    Log.Info("Sending to " + Exchange + " with key " + routingKey, this);
                    channel.BasicPublish(Exchange, routingKey, null, body);
                }
            }

            Debug.WriteLine("SaveItemProcessor: " + args.Item.Paths.FullPath + ", " + args.Item.Database.Name);
        }
    }
}