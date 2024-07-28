using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Address.API.Import.Xml.Update
{
    public class Curentst : Importer
    {
        public Curentst(NpgsqlConnection connection, string xmlPath, bool deleteRecords) :base(connection, xmlPath, deleteRecords)
        {
            KeyField = "curentstid";
            TableName = "curentst";
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
                        // исправление косяков товарищей из nalog.ru
                        // Apostrophe is not applicable in sql string constants
                        resultRecord.Add(attribute.Name.LocalName.ToLower(), attribute.Value.Replace('\'', '\"'));
                    }

                    return resultRecord;
                });
        }
    }
}
