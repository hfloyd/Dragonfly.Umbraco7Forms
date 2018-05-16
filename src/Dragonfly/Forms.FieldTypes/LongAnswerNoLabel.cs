namespace Dragonfly.Forms.FieldTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;
    using Umbraco.Forms.Core;

    public class LongAnswerNoLabel : Umbraco.Forms.Core.Providers.FieldTypes.Textarea
    {
        public LongAnswerNoLabel()
        {
            // Mandatory
            this.Id = new Guid("FD7A8788-B06C-42E4-9738-DE22F638C982");
            this.Name = "Long Answer (No Label)";
            this.Description = "Long answer text area with hidden label";
            //this.Icon = "icon-medal";
            //this.DataType = FieldDataType.String;
            this.FieldTypeViewName = "FieldType.Textarea.cshtml";

            // Optional         
            this.Category = "Custom Types";
            this.HideField = false;
            this.HideLabel = true;
            this.SortOrder = 10;
            this.SupportsPrevalues = false;
            this.SupportsRegex = true;

        }
    }
}