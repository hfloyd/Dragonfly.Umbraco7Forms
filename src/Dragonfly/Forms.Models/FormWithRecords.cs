using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Forms.Models
{
    using Umbraco.Forms.Core;
    using Umbraco.Forms.Core.Enums;
    using Umbraco.Forms.Data.Storage;
    using Umbraco.Forms.Mvc.DynamicObjects;

    public class FormWithRecords
    {
        public Guid FormGuid { get; }
        public Umbraco.Forms.Core.Form Form { get; }
        public IEnumerable<string> FormInfo { get; }
        //public string FormName { get;  }
        public IEnumerable<Umbraco.Forms.Core.Field> Fields { get;  }
        public List<Record> RecordsAll { get; }
        public List<Record> RecordsApproved { get;}

        public FormWithRecords(string FormGuidString)
        {
            //Set basic properties
            this.FormGuid = new Guid(FormGuidString);

            Record temp; 
            //These versions cause "There is already an open DataReader associated with this Command which must be closed first." errors
            //see: https://our.umbraco.org/forum/umbraco-forms/78207-working-with-record-data-there-is-already-an-open-datareader-associated
            //this.RecordsApproved = Library.GetApprovedRecordsFromForm(FormGuidString).Items;
            //this.RecordsAll = Library.GetRecordsFromForm(FormGuidString).Items;
            //Alternative:
            using (var formStorage = new FormStorage())
            {
                using (var recordStorage = new RecordStorage())
                {
                    var form = formStorage.GetForm(this.FormGuid);
                    var allRecords = recordStorage.GetAllRecords(form).ToList();

                    this.RecordsAll = allRecords;
                    this.RecordsApproved = allRecords.Where(x => x.State == FormState.Approved).ToList();
                }
            }

            // Get form info
            using (FormStorage formStorage = new FormStorage())
            {
                this.Form = formStorage.GetForm(this.FormGuid);
            }

            //Get all fields
            var fields = new List<Field>();
            var exampleRecord = RecordsAll.First();
            foreach (var fieldItem in exampleRecord.RecordFields)
            {
                var recField = fieldItem.Value;
                var field = recField.Field;
                fields.Add(field);
            }
            this.Fields = fields;
        }

        public IEnumerable<KeyValuePair<Guid, RecordField>> AllFieldData(string FieldAlias, bool ApprovedOnly = true)
        {
            var returnData = new List<KeyValuePair<Guid, RecordField>>();

            var records = ApprovedOnly ? this.RecordsApproved : this.RecordsAll;

            foreach (var record in records)
            {
                var match = record.RecordFields.Where(n => n.Value.Alias == FieldAlias).First();

                returnData.Add(match);
            }

            return returnData;

        }
    }
}
