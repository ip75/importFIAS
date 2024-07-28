using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Address.API.Import.Xml.Update
{
    public class Normdoc : Importer
    {
        public Normdoc(NpgsqlConnection connection, string xmlPath, bool deleteRecords) :base(connection, xmlPath, deleteRecords)
        {
            KeyField = "normdocid";
            TableName = "normdoc";
        }

        public override async Task Run(CancellationToken cancellationToken)
        {
            if (DeleteRecords)
                await Delete(cancellationToken);
            else
                await base.ImportXml(cancellationToken, (xmlRecord) =>
                {
                    var resultRecord = new Dictionary<string, string>();
                    foreach (var attribute in xmlRecord?.Attributes())
                    {
                        // Apostrophe is not applicable in sql string constants
                        resultRecord.Add(attribute.Name.LocalName.ToLower(), attribute.Value.Replace('\'', '\"'));
                    }

                    // исправление косяков товарищей из nalog.ru
                    if (resultRecord.ContainsKey("docname") && resultRecord["docname"].Length > 128)
                    {
                        resultRecord["docname"] = resultRecord["docname"].Substring(0, 128);
                    }
                    if (resultRecord.ContainsKey("docnum") && resultRecord["docnum"].Length > 20)
                    {
                        resultRecord["docnum"] = resultRecord["docnum"].Substring(0, 20);
                    }

                    return resultRecord;
                });
        }
    }
}
