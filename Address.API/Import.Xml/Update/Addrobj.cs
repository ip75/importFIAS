using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Address.API.Import.Xml.Update
{
    public class Addrobj : Importer
    {
        public Addrobj(NpgsqlConnection connection, string xmlPath, bool deleteRecords) : base(connection, xmlPath, deleteRecords)
        {
            KeyField = "aoid";
            TableName = "addrobj";
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
                    if (resultRecord.ContainsKey("currstatus") && resultRecord["currstatus"] == string.Empty)
                    {
                        resultRecord["currstatus"] = "0";
                    }

                    return resultRecord;
                });

            return;

            // Entity import
            await EFImoprtXml(reader =>
            {
                Model.Addrobj addressObject = (Model.Addrobj) new XmlSerializer(typeof(Model.Addrobj)).Deserialize(reader);

//              var currentKey = addressObjectXml?.Attributes().First(field => string.Compare(field.Name.LocalName, "aoid", StringComparison.CurrentCultureIgnoreCase) == 0)?.Value;
                if (Context.Addrobj.Any(obj => obj.Aoid == addressObject.Aoid))
                {
                    // UPDATE
                    Context.Addrobj.Update(addressObject);
                }
                else
                {
                    // INSERT
                    Context.Addrobj.Add(addressObject);
                }

            });
        }
    }
}
