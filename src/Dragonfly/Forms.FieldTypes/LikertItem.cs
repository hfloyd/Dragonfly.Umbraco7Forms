namespace Dragonfly.Forms.FieldTypes
{
    using System;
    using System.Collections.Generic;
    using Umbraco.Forms.Core;

    public class LikertItem: Umbraco.Forms.Core.FieldType
    {
        [Umbraco.Forms.Core.Attributes.Setting("Maximum Scale Number",
            description = "Scale will be 1 to Max Number (Default is 5)",
            alias = "MaxScaleNum",
            view = "Textfield")]
        public string MaxScaleNumString { get; set; }

        public const int MaxScaleNumDefault = 5;

        public int MaxScaleNum
        {
            get
            {
                int MaxScaleNum = MaxScaleNumDefault;
                var isNum = Int32.TryParse(MaxScaleNumString, out MaxScaleNum);

                return MaxScaleNum;
            }
        }

        [Umbraco.Forms.Core.Attributes.Setting("Include 'N/A' Option?",
            description = "(It will return a value of 0)",
            alias = "IncludeNaOption",
            view="CheckBox")]
        public string IncludeNaOptionString { get; set; }

        public bool IncludeNaOption {
            get
            {
                if (IncludeNaOptionString == "True")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Umbraco.Forms.Core.Attributes.Setting("'N/A' Option Text",
            description = "Default if unspecified is 'N/A'",
            alias = "NaOptionText",
            view = "Textfield")]
        public string NaOptionText { get; set; }

        public const string NaOptionDefaultText = "N/A";


        public LikertItem()
        {
            // Mandatory
            this.Id = new Guid("5CF57666-565D-46B8-AB88-55CB9E2A5E01");
            this.Name = "Likert Rating Item";
            this.Description = "Renders a single Likert Rating input";
            this.Icon = "icon-bars";
            this.DataType = FieldDataType.Integer;
            this.FieldTypeViewName = "FieldType.LikertItem.cshtml";

            // Optional      
            this.Category = "Custom Types";
            this.HideField = false;
            this.HideLabel = false;
            this.SortOrder = 10;
            this.SupportsPrevalues = false;
            this.SupportsRegex = false;
        }

        public Dictionary<int, string> RatingOptions()
        {
            var ratingOptions = new Dictionary<int, string>();

            for (int i = 1; i < MaxScaleNum + 1; i++)
            {
                ratingOptions.Add(i, i.ToString());
            }

            if (IncludeNaOption)
            {
                var naText = NaOptionText != "" ? NaOptionText : NaOptionDefaultText;
                ratingOptions.Add(0, naText);
            }

            return ratingOptions;
        }
    }
}