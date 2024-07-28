using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Address.API.Import.Xml.Update
{
    public class House : Importer
    {
        public House(NpgsqlConnection connection, string xmlPath, bool deleteRecords) :base(connection, xmlPath, deleteRecords)
        {
            KeyField = "houseid";
            TableName = "house";
        }

        /// <summary>
        /// в одной из колонок встречаются значения больше чем максимальное возможное по схеме. Там просто кусок памяти с текстом.
        /// чуваки из nalog.ru не валидируют свои xml при выгрузке
        /// value too long for type character varying(10)
        /// buildnum   | character varying(10)
        /// strucnum   | character varying(10)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
                    if (resultRecord.ContainsKey("strucnum") && resultRecord["strucnum"].Length > 10)
                        resultRecord["strucnum"] = resultRecord["strucnum"].Substring(0, 10);

                    return resultRecord;
                });
        }
    }
}
