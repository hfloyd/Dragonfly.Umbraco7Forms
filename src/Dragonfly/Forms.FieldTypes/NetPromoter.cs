namespace Dragonfly.Forms.FieldTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Dragonfly.Forms.Models;
    using Newtonsoft.Json;
    using Umbraco.Forms.Core;

    public class NetPromoter : Umbraco.Forms.Core.FieldType
    {
        public NetPromoter()
        {
            // Mandatory
            this.Id = new Guid("F3989404-49CB-484A-9FCB-957278A23CFF");
            this.Name = "Net Promoter Score (NPS®)";
            this.Description = "Scale of 0 - 10 for NPS";
            this.Icon = "icon-medal";
            this.DataType = FieldDataType.String;
            this.FieldTypeViewName = "FieldType.NetPromoter.cshtml";

            // Optional          
            this.Category = "Custom Types";
            this.HideField = false;
            this.HideLabel = false;
            this.SortOrder = 10;
            this.SupportsPrevalues = false;
            this.SupportsRegex = false;

        }

        public string Default0Label = "Not at All Likely";
        public string Default10Label = "Very Likely";

        public Dictionary<int, string> RatingOptions()
        {
            var ratingOptions = new Dictionary<int, string>();

            for (int i = 0; i < 11; i++)
            {
                ratingOptions.Add(i, i.ToString());
            }

            return ratingOptions;
        }

        public override IEnumerable<object> ProcessSubmittedValue(Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            List<object> returnValue = new List<object>();

            if (postedValues.Any())
            {
                var valueString = postedValues.First().ToString();
                var value = 0;
                var valueValid = Int32.TryParse(valueString, out value);

                if (!valueValid)
                {
                    var invalidNps = new NetPromoterRating(true);
                    returnValue.Add(invalidNps.ToString());
                }
                else
                {
                    var nps = new NetPromoterRating(value);
                    returnValue.Add(nps.ToString());
                }
            }
            return returnValue;
        }

        #region Overrides of FieldType

        public override IEnumerable<object> ConvertFromRecord(Field field, IEnumerable<object> storedValues)
        {
            return base.ConvertFromRecord(field, storedValues);
        }

        #endregion
    }

}