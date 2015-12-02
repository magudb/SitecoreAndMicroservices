using System.Collections.Generic;

namespace Web.CM.Models
{
    public class ItemModel
    {
        public ItemPropertyModel Properties { get; set; }
        public IEnumerable<FieldModel> Fields { get; set; }
        public IEnumerable<ItemModel> Children { get; set; }
    }
}