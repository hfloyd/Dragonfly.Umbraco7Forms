namespace Dragonfly.Forms.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Dragonfly.Forms.Models;
    using umbraco.presentation.umbraco.create;
    using Umbraco.Core.Logging;
    using Umbraco.Forms.Core;
    using Umbraco.Forms.Core.Enums;
    using Umbraco.Forms.Mvc.DynamicObjects;

    public static class FormHelper
    {
        [Obsolete("No longer needed with non-Dynamic record fetching.")]
        public static string GetRecordGuid(string FormGuid, string RecordId)
        {
            var recordSet = Library.GetRecordsFromForm(FormGuid);

            foreach (dynamic record in recordSet)
            {
                try
                {
                    if (record.Id.ToString() == RecordId.ToString())
                    {
                        return record.UniqueId.ToString();
                    }
                }
                catch (Exception e)
                {
                    var msg = string.Format("FormHelper.GetRecordGuid for Form '{0}' and Record '{1}' failed.", FormGuid,RecordId);
                    LogHelper.Error<string>(msg, e);
                    return "";
                }
            }

            //Not found
            var msg2 = string.Format("FormHelper.GetRecordGuid for Form '{0}' - Unable to find match for Record '{1}'.", FormGuid, RecordId);
            LogHelper.Warn<string>(msg2);
            return "";
        }

        public static string GetStringFieldValue(Record Record, string FieldAlias)
        {
            var returnValue = "";

            try
            {
                var fieldValues = Record.RecordFields.Values.Where(n => n.Alias == FieldAlias).ToList();
                if (fieldValues.Any())
                {
                    returnValue = fieldValues.FirstOrDefault().ValuesAsString();
                }
                else
                {
                    var msg = $"ERROR on record # {Record.Id} - No field with alias '{FieldAlias}' found.";
                    LogHelper.Warn<string>(msg);
                }
            }
            catch (Exception e)
            {
                var msg = $"ERROR on record # {Record.Id} - Field conversion for '{FieldAlias}'";
                LogHelper.Error<string>(msg, e);
            }

            return returnValue;
        }

        public static int GetIntFieldValue(Record Record, string FieldAlias)
        {
            var returnValue = 0;

            try
            {
                var fieldValues = Record.RecordFields.Values.Where(n => n.Alias == FieldAlias).ToList();
                if (fieldValues.Any())
                {
                    var intString = fieldValues.FirstOrDefault().ValuesAsString();
                    var isNumeric = Int32.TryParse(intString, out returnValue);

                    if (!isNumeric)
                    {
                        var msg = $"ERROR on record # {Record.Id} - Field '{FieldAlias}' with value '{intString}' could not be converted to an integer.";
                        LogHelper.Warn<int>(msg);
                    }
                }
                else
                {
                    var msg = $"ERROR on record # {Record.Id} - No field with alias '{FieldAlias}' found.";
                    LogHelper.Warn<int>(msg);
                }
            }
            catch (Exception e)
            {
                var msg = $"ERROR on record # {Record.Id} - Field conversion for '{FieldAlias}'";
                LogHelper.Error<int>(msg, e);
            }

            return returnValue;
        }

    }



    public class FormDataMyForm
    {
        public FormWithRecords FormWithRecords { get; internal set; }
        public IEnumerable<FormDataMyFormRecord> Records { get; internal set; }

        public FormDataMyForm(string FormGuid)
        {
            this.FormWithRecords = new FormWithRecords(FormGuid);

            var formRecords = new List<FormDataMyFormRecord>();
            foreach (var record in FormWithRecords.RecordsAll)
            {
                var intValue = 0;
                var intTest = false;

                var formGuid = new Guid(FormGuid);
                var typedRecord = new FormDataMyFormRecord();

                //Standard Record Info
                typedRecord.RecordId = record.Id;
                typedRecord.State = record.State;
                typedRecord.RecordUniqueId = record.UniqueId;
                typedRecord.Created = record.Created;
                typedRecord.IP = record.IP;
                typedRecord.MemberKey = record.MemberKey;
                typedRecord.UmbracoPageId = record.UmbracoPageId;
                typedRecord.Updated = record.Updated;

                //Custom Field Values
                typedRecord.StringAlias = record.GetRecordField("StringAlias").ValuesAsString();

                intValue = 0;
                intTest = Int32.TryParse(record.GetRecordField("IntAlias").ValuesAsString(), out intValue);
                typedRecord.IntAlias = intValue;

                typedRecord.DateAlias = DateTime.Parse(record.GetRecordField("DateAlias").ValuesAsString());

                formRecords.Add(typedRecord);
            }
        }
  
    }

    public class FormDataMyFormRecord
    {
        public string IP {get; internal set; }
        public Guid RecordUniqueId { get; internal set; }
        public int RecordId { get; internal set; }
        public FormState? State { get; internal set; }
        public object MemberKey { get; internal set; }
        public int UmbracoPageId { get; internal set; }
        public DateTime Created { get; internal set; }
        public DateTime Updated { get; internal set; }

        public string StringAlias { get; internal set; }
        public int IntAlias { get; internal set; }
        public DateTime DateAlias { get; internal set; }
        

        public FormDataMyFormRecord() { }
    }
}
