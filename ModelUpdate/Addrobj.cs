using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using importFias;
using Npgsql;
using ShellProgressBar;

namespace FIAS
{
    public partial class Addrobj
    {
        public static async Task ImportXml(string xmlPath, string pgConnectionString, CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection("Host=localhost;Database=fias;Username=lt;Password=1");
            await connection.OpenAsync(cancellationToken);

            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                using var bar = new ProgressBar(100000, "import file to database", new ProgressBarOptions {ProgressCharacter = '─', ProgressBarOnBottom = true});
                var skipped = 0;
                var keyField = "aoid";
                var updateReady = false;

                using var reader = XmlReader.Create(xmlPath, new XmlReaderSettings {Async = true});

                await reader.MoveToContentAsync();


                var updateFields = string.Empty;
                var updateValues = string.Empty;
                var insertRow = new Dictionary<string, string>();

                while (await reader.ReadAsync())
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                        break;

                    var addressObjectXml = await XNode.ReadFromAsync(reader, CancellationToken.None) as XElement;

                    foreach (var attribute in addressObjectXml?.Attributes())
                    {
                        insertRow.Add(attribute.Name.LocalName.ToLower(), attribute.Value.Replace('\'', '\"'));
                    }

                    // исправление косяков товарищей из nalog.ru, 10% записей приходит с нулевой строкой вместо числа 0
                    if (insertRow["currstatus"] == string.Empty)
                    {
                        insertRow["currstatus"] = "0";
                    }

                    if (!updateReady)
                    {
                        updateFields = string.Join(",", insertRow.Keys.Where(field => field != keyField));
                        updateValues = string.Join(", EXCLUDED.", insertRow.Keys.Where(field => field != keyField));
                        updateReady = true;
                    }

                    await using (var cmd = new NpgsqlCommand($"INSERT INTO addrobj ({string.Join(",", insertRow.Keys)})" +
                                                             $" VALUES ({string.Join(",", insertRow.Values.ToList().Stringify())})" +
                                                             $" ON CONFLICT ({keyField})" +
                                                             $" DO UPDATE SET" +
                                                             $" ({updateFields}) = ROW (EXCLUDED.{updateValues})", connection))
                    {
                        //cmd.Parameters.AddWithValue("p", "Hello world");
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                        insertRow.Clear();
                    }

                    if (bar.MaxTicks == bar.CurrentTick)
                    {
                        bar.MaxTicks += bar.MaxTicks;
                    }

                    bar.Tick();
                }

                await transaction.CommitAsync(cancellationToken);
                bar.Tick(bar.MaxTicks);
                Console.WriteLine($"{bar.MaxTicks} records updated, {skipped} skipped because of errors, from file {xmlPath}");
            }
            catch (PostgresException pgException)
            {
                await transaction.RollbackAsync(cancellationToken);
                Console.WriteLine(pgException);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                Console.WriteLine(ex);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}
