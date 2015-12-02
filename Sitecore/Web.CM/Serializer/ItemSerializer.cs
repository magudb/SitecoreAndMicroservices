using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Web.CM.Models;

namespace Web.CM.Serializer
{
    public class ItemSerializer
    {
        public string SerializeItem(Item item,  string device,
                                  string[] itemFields = null,
                                  string[] childFields = null,
                                  string mediaBaseUrl = null)
        {
            var @object = CreateResponseModelForItem(item, itemFields,  mediaBaseUrl);
            return JsonConvert.SerializeObject(@object);
        }
      
        private ServerResponseModel CreateResponseModelForItem(Item item, string[] itemFields, string mediaBaseUrl)
        {
            return new ServerResponseModel
            {
                Items = new[]
                {
                    CreateItemModel(item, itemFields, mediaBaseUrl)
                }
            };
        }

        private ItemModel CreateItemModel(Item item, string[] itemFields,  string mediaBaseUrl)
        {
            return new ItemModel
            {
                Properties = MapItem(item),
                Fields = MapFields(item, itemFields, mediaBaseUrl),
                Children = item.GetChildren().Select(child => CreateItemModel(child, itemFields, mediaBaseUrl))
            };
        }

        private ItemPropertyModel MapItem(Item item)
        {
            return new ItemPropertyModel
            {
                Id = item.ID.Guid,
                FullPath = item.Paths.FullPath.ToLowerInvariant(),
                Language = item.Language.Name,
                Name = item.Name,
                ParentId = item.ParentID.Guid,
                TemplateId = item.TemplateID.Guid,
                HasVersion = item.Versions.Count > 0
            };
        }
       
        private IEnumerable<FieldModel> MapFields(Item item, string[] specificFields, string mediaBaseUrl)
        {
            if (item.Versions.Count == 0)
            {
                return Enumerable.Empty<FieldModel>();
            }
            var fields = new List<FieldModel>();
            if (specificFields == null || !specificFields.Any())
            {
                return item.Fields.Where(f => !f.Key.StartsWith("__")).Select(f => MapField(f, mediaBaseUrl)).Where(f => f != null);
            }
            foreach (var key in specificFields)
            {
                var f = item.Fields[key.Trim()];
                if (f != null)
                {
                    fields.Add(MapField(f, mediaBaseUrl));
                }
            }
            return fields.Where(f => f != null);
        }
        private FieldModel MapField(Field field, string mediaBaseUrl)
        {
            object value;
            if (field.TypeKey.Equals("image"))
            {
                var media = (ImageField)field;
                if (media.MediaItem == null)
                {
                    return null;
                }
                value = new ImageFieldValueModel
                {
                    Alt = media.Alt,
                    Url = MediaManager.GetMediaUrl(media.MediaItem, new MediaUrlOptions
                    {
                        MediaLinkServerUrl = mediaBaseUrl,
                        AlwaysIncludeServerUrl = true,
                        IncludeExtension = true,
                        LowercaseUrls = true,
                        UseItemPath = true
                    })
                };
            }
            else if (field.TypeKey.Equals("general link"))
            {
                var link = (LinkField)field;
                value = new LinkFieldValueModel
                {
                    Description = link.Text,
                    TargetId = link.IsInternal ? link.TargetID.Guid : Guid.Empty,
                    TargetUrl = link.IsInternal ? "" : link.GetFriendlyUrl()
                };
            }
            else
            {
                value = field.Value;
            }
            return new FieldModel
            {
                Id = field.ID.Guid,
                Type = field.TypeKey,
                Key = field.Key,
                Value = value
            };
        }
    }
}