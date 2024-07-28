using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Address.API.Import.Xml.Update
{
    public class Stead : Importer
    {
        public Stead(NpgsqlConnection connection, string xmlPath, bool deleteRecords) :base(connection, xmlPath, deleteRecords)
        {
            KeyField = "steadid";
            TableName = "stead";
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
                    if (resultRecord.ContainsKey("number") && resultRecord["number"].Length > 120)
                        resultRecord["number"] = resultRecord["number"].Substring(0, 120);

                    return resultRecord;
                });
        }
    }
}
