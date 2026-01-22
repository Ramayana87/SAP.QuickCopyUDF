using Newtonsoft.Json;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAP.QuickCopyUDF
{

    public class ValidValuesMDImpl
    {
        public string Description { get; set; }

        public string Value { get; set; }
    }
    public class UserFieldsImpl
    {
        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public long EditSize { get; set; }

        public long FieldID { get; set; }

        public string LinkedTable { get; set; }

        public string LinkedUDO { get; set; }

        public string LinkedObject { get; set; }

        public BoYesNoEnum Mandatory { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public BoFldSubTypes SubType { get; set; }

        public string TableName { get; set; }

        public BoFieldTypes Type { get; set; }

        public List<ValidValuesMDImpl> ValidValues { get; set; }

        public object Value { get; set; }
    }

    public class UserObjectsImpl
    {
        public BoYesNoEnum RebuildEnhancedForm { get; set; }
        public int Position { get; set; }
        public BoYesNoEnum OverwriteDllfile { get; set; }
        public BoUDOObjType ObjectType { get; set; }
        public string Name { get; set; }
        public string MenuUID { get; set; }
        public BoYesNoEnum MenuItem { get; set; }
        public string MenuCaption { get; set; }
        public BoYesNoEnum ManageSeries { get; set; }
        public string LogTableName { get; set; }
        public string FormSRF { get; set; }
        public List<UserObjectFormColumnImpl> FormColumns { get; set; }
        public List<UserObjectFindColumnImpl> FindColumns { get; set; }
        public int FatherMenuID { get; set; }
        public string ExtensionName { get; set; }
        public List<UserObjectEnhancedFormColumnImpl> EnhancedFormColumns { get; set; }
        public BoYesNoEnum EnableEnhancedForm { get; set; }
        public string Code { get; set; }
        public List<UserObjectChildTableImpl> ChildTables { get; set; }
        public BoYesNoEnum CanYearTransfer { get; set; }
        public BoYesNoEnum CanLog { get; set; }
        public BoYesNoEnum CanFind { get; set; }
        public BoYesNoEnum CanDelete { get; set; }
        public BoYesNoEnum CanCreateDefaultForm { get; set; }
        public BoYesNoEnum CanClose { get; set; }
        public BoYesNoEnum CanCancel { get; set; }
        public BoYesNoEnum CanArchive { get; set; }
        public string TableName { get; set; }
        public BoYesNoEnum UseUniqueFormType { get; set; }
    }

    public class UserObjectChildTableImpl
    {
        public string Code { get; set; }
        public int Count { get; set; }
        public string LogTableName { get; set; }
        public string ObjectName { get; set; }
        public int SonNumber { get; set; }
        public string TableName { get; set; }
    }

    public class UserObjectFormColumnImpl
    {
        public string Code { get; set; }
        public BoYesNoEnum Editable { get; set; }
        public string FormColumnAlias { get; set; }
        public string FormColumnDescription { get; set; }
        public int FormColumnNumber { get; set; }
        public int SonNumber { get; set; }
    }

    public class UserObjectFindColumnImpl
    {
        public string Code { get; set; }
        public string ColumnAlias { get; set; }
        public string ColumnDescription { get; set; }
        public int ColumnNumber { get; set; }
    }

    public class UserObjectEnhancedFormColumnImpl
    {
        public int ChildNumber { get; set; }
        public string Code { get; set; }
        public string ColumnAlias { get; set; }
        public string ColumnDescription { get; set; }
        public BoYesNoEnum ColumnIsUsed { get; set; }
        public int ColumnNumber { get; set; }
        public BoYesNoEnum Editable { get; set; }
    }

    //public static class SqlHelper
    //{
    //    public static void TryAddParameters(this SqlCommand command, params SqlParameter[] parameters)
    //    {
    //        if (command == null)
    //            throw new ArgumentNullException("command");
    //        if (parameters == null || parameters.Length == 0) return;
    //        command.Parameters.AddRange(parameters);
    //    }
    //}
    public class LayerMessageObject
    {
        public LayerErrorObject error { get; set; }
    }

    public class LayerErrorObject
    {
        public int code { get; set; }

        public MessageObject message { get; set; }
    }

    public class MessageObject
    {
        public string lang { get; set; }
        public string value { get; set; }
    }


    public class SessionObj
    {
        public string SessionId { get; set; }
        //odata.metadata
        [JsonProperty(PropertyName = "odata.metadata")]
        public string BaseUrl { get; set; }
        public string Version { get; set; }
        public int SessionTimeout { get; set; }
    }
}
