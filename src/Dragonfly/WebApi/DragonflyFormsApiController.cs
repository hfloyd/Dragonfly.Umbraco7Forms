namespace Dragonfly.WebApi
{
    using System.Text;
    using System.Net.Http;
    using Dragonfly.Forms.Models;
    using Dragonfly.NetHelpers;
    using Umbraco.Forms.Core;
    using Umbraco.Web.WebApi;

    // /Umbraco/Api/DragonflyFormsApi <-- UmbracoApiController
    // /Umbraco/backoffice/Api/DragonflyFormsApi <-- UmbracoAuthorizedApiController

    [IsBackOffice]
    public class DragonflyFormsApiController : UmbracoApiController
    {
        /// /umbraco/backoffice/api/DragonflyFormsApi/Test
        [System.Web.Http.AcceptVerbs("GET")]
        public bool Test()
        {
            //LogHelper.Info<DragonflyFormsApiController>("Test STARTED/ENDED");
            return true;
        }

        /// /umbraco/backoffice/api/DragonflyFormsApi/GenerateFormClass?FormGuid=xxx
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage GenerateFormClass(string FormGuid)
        {
            var returnSB = new StringBuilder();

            var formData = new FormWithRecords(FormGuid);
            var formClass = new StringBuilder();
            var recordClass = new StringBuilder();

            var formClassName = formData.Form.Name.MakeCodeSafe("",true);

            //TODO: HLF - Update with new Record and FormsHelper syntax

            formClass.AppendLine($@"
public partial class Form{formClassName}
    {{
        public FormWithRecords FormWithRecords {{ get; internal set; }}
        public IEnumerable<Form{formClassName}Record> Records {{ get; internal set; }}

        public Form{formClassName}(string FormGuid)
        {{
            this.FormWithRecords = new FormWithRecords(FormGuid);

            var formRecords = new List<Form{formClassName}Record>();
            foreach (var record in FormWithRecords.RecordsAll)
            {{
                var intValue = 0;
                var intTest = false;

                var formGuid = new Guid(FormGuid);
                var typedRecord = new Form{formClassName}Record();

                //Standard Record Info
                typedRecord.RecordId = record.Id;
                typedRecord.State = record.State;
                typedRecord.RecordUniqueId = FormHelper.GetRecordGuid(FormGuid, record.Id);
                typedRecord.Created = record.Created;
                typedRecord.IP = record.IP;
                typedRecord.MemberKey = record.MemberKey;
                typedRecord.UmbracoPageId = record.UmbracoPageId;
                typedRecord.Updated = record.Updated;

                //Custom Field Values
            ");

            recordClass.AppendLine($@"
            public class Form{formClassName}Record
    {{
        public string IP {{get; internal set; }}
        public string RecordUniqueId {{ get; internal set; }}
        public string RecordId {{ get; internal set; }}
        public FormState? State {{ get; internal set; }}
        public object MemberKey {{ get; internal set; }}
        public int UmbracoPageId {{ get; internal set; }}
        public DateTime Created {{ get; internal set; }}
        public DateTime Updated {{ get; internal set; }}
                                   ");

            foreach (var field in formData.Fields)
            {
                var fieldAlias = field.Alias;
                if (field.FieldType.DataType == FieldDataType.String ||
                    field.FieldType.DataType == FieldDataType.LongString)
                {

                    //Add field to contructor for form class
                    formClass.AppendLine($@"
                typedRecord.{fieldAlias} = record.GetField(""{fieldAlias}"").ValuesAsString();
                                   ");

                    //Add field as property to record class
                    recordClass.AppendLine($@"
                public string {fieldAlias} {{ get; internal set; }}
                                   ");
                }
                else if (field.FieldType.DataType == FieldDataType.Integer)
                {
                    //Add field to contructor for form class
                    formClass.AppendLine($@"
                  intValue = 0;
                intTest = Int32.TryParse(record.GetField(""{fieldAlias}"").ValuesAsString(), out intValue);
                typedRecord.{fieldAlias} = intValue;
                    ");

                    //Add field as property to record class
                    recordClass.AppendLine($@"
                public int {fieldAlias} {{ get; internal set; }}
                                   ");
                }
                else if(field.FieldType.DataType == FieldDataType.DateTime)
                {

                    //Add field to contructor for form class
                    formClass.AppendLine($@"
                typedRecord.{fieldAlias} = DateTime.Parse(record.GetField(""{fieldAlias}"").ValuesAsString());
                                   ");

                    //Add field as property to record class
                    recordClass.AppendLine($@"
                public DateTime {fieldAlias} {{ get; internal set; }}
                                   ");
                }
            }

            //Finalize the classes and combine
                recordClass.AppendLine($@"
                    public Form{formClassName}Record() {{ }}
    }}
                                   ");
                
                formClass.AppendLine($@"
            }}
        }}
  
    }}
                                   ");

                returnSB.Append(formClass);
                returnSB.AppendLine("");
                returnSB.Append(recordClass);


            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    returnSB.ToString(),
                    Encoding.UTF8,
                    "text/html"
                )
            };
        }
    }
}
