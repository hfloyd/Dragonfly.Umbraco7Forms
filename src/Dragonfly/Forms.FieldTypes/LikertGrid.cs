namespace Dragonfly.Forms.FieldTypes
{
    using System;
    using Umbraco.Forms.Core;

    public class LikertGrid: Umbraco.Forms.Core.FieldType
    {
        public LikertGrid()
        {
            //Provider
            this.Id = new Guid("78F7BA76-0C8D-48F5-8516-49997105351F");
            this.Name = "LikertGrid";
            this.Description = "Renders a Likert Grid input";
            this.Icon = "icon-poll";
            this.DataType = FieldDataType.LongString;
            this.SortOrder = 11;
        }
    }
}