using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Address.API.Model;
using Serilog;
using Npgsql;

namespace Address.API.Import.Xml
{
    public abstract class Importer
    {
        /// <summary>
        /// Connection object Npgsql initialized on start of update process
        /// </summary>
        protected NpgsqlConnection Connection { get; }
        /// <summary>
        /// Path to xml file which we have to import to database
        /// </summary>
        protected string XmlPath { get; }
        /// <summary>
        /// Update also can contain files where described records which have to be deleted. Corrupted or wrong ones. Files with "_DEL_" prefix.
        /// </summary>
        protected bool DeleteRecords { get; }

        protected string KeyField { get; set; }
        protected string TableName { get; set; }

        protected fiasContext Context { get; set; } = new fiasContext();

        protected Importer(NpgsqlConnection connection, string xmlPath, bool deleteRecords = false)
        {
            Connection = connection;
            XmlPath = xmlPath;
            DeleteRecords = deleteRecords;
        }

        public abstract Task Run(CancellationToken cancellationToken);

        public virtual async Task Delete(CancellationToken cancellationToken)
        {
            Log.Logger.Information($"starting import data from {XmlPath} to table {TableName}");
            await Connection.OpenAsync(cancellationToken);

            using var reader = XmlReader.Create(XmlPath, new XmlReaderSettings {Async = true});

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                    break;

                var xmlRecord = await XNode.ReadFromAsync(reader, cancellationToken) as XElement;
                var indexValue = xmlRecord?.Attributes().First(attribute => attribute.Name.LocalName.ToLower() == KeyField).Value;

                if (string.IsNullOrEmpty(indexValue))
                {
                    continue;
                }

                await using var cmd = new NpgsqlCommand($"DELETE FROM {TableName} WHERE {KeyField} = {indexValue}", Connection);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await Connection.CloseAsync();

            Log.Logger.Information($"Delete records mentioned in file {XmlPath}");
        }

        public virtual async Task ImportXml(CancellationToken cancellationToken, Func<XElement, Dictionary<string,string>> fixRecordFunc)
        {
            Log.Logger.Information($"starting import data from {XmlPath} to table {TableName}");
            await Connection.OpenAsync(cancellationToken);

            var transaction = await Connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // this parameter also can be set by connection string "Search Path:filas,public"
                //await new NpgsqlCommand("SET search_path TO public,fias", Connection).ExecuteNonQueryAsync(cancellationToken);
                //cmd.Parameters.AddWithValue("p", "Hello world");
                
                var skipped = 0;
                var upserted = 0;
                var updateReady = false;

                using var reader = XmlReader.Create(XmlPath, new XmlReaderSettings {Async = true});

                await reader.MoveToContentAsync();

                var updateFields = string.Empty;
                var updateValues = string.Empty;

                while (await reader.ReadAsync())
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                        break;

                    var xmlRecord = await XNode.ReadFromAsync(reader, cancellationToken) as XElement;

                    // исправление косяков товарищей из nalog.ru
                    var insertRow = fixRecordFunc(xmlRecord);

                    if (!updateReady)
                    {
                        updateFields = string.Join(",", insertRow.Keys.Where(field => field != KeyField));
                        updateValues = string.Join(", EXCLUDED.", insertRow.Keys.Where(field => field != KeyField));
                        updateReady = true;
                    }

                    await using var cmd = new NpgsqlCommand($"INSERT INTO {TableName} " +
                                                            $"({string.Join(",", insertRow.Keys)})" +
                                                            $" VALUES " +
                                                            $"({string.Join(",", insertRow.Values.ToList().Stringify())})" +
                                                            $" ON CONFLICT ({KeyField})" +
                                                            $" DO UPDATE SET" +
                                                            $" ({updateFields}) = ROW (EXCLUDED.{updateValues})", Connection, transaction);
                    //cmd.Parameters.AddWithValue("p", "Hello world");
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    insertRow.Clear();
                    upserted++;
                }

                await transaction.CommitAsync(cancellationToken);
                Log.Logger.Information($"{upserted} records updated, {skipped} skipped because of errors, from file {XmlPath}");
            }
            catch (PostgresException pgException)
            {
                await transaction.RollbackAsync(cancellationToken);
                Log.Logger.Error(pgException, $"table name: {TableName}\nxml file: {XmlPath}\nDeleteRecords: {DeleteRecords}");
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                Log.Logger.Error(ex, $"table name: {TableName}\nxml file: {XmlPath}\nDeleteRecords: {DeleteRecords}");
            }
            finally
            {
                await Connection.CloseAsync();
                Log.Logger.Information($"successfully import file {XmlPath} to table : {TableName}");
            }
        }

        /// <summary>
        /// только когда полубоги из налоговой снизойдут до нас смердов и
        /// станут выгружать обновления в соответствии со своей же xsd схемой базы,
        /// только тогда можно будет пользоваться этим методом. Entity Framework
        /// </summary>
        /// <param name="upsertRecord"></param>
        /// <returns></returns>
        public virtual async Task EFImoprtXml(Action<XmlReader> upsertRecord)
        {
            using var reader = XmlReader.Create(XmlPath, new XmlReaderSettings {Async = true});

            await reader.MoveToContentAsync();

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                    break;
                upsertRecord(reader);

                await Context.SaveChangesAsync();
            }
        }
    }
}
