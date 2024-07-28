using FIAS;
using Npgsql;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;

namespace importFias
{
    class Program
    {
        public static string GetTableName(string xmlObjectName)
        {
            switch (xmlObjectName)
            {
                case "AddressObjects":
                    return "addrobj";
                //case "":
                //    return "actstat";
                //case "":
                //    return "addrobj";
                //case "":
                //    return "centerst";
                //case "":
                //    return "curentst";
                //case "":
                //    return "eststat";
                //case "":
                //    return "flattype";
                //case "":
                //    return "house";
                //case "":
                //    return "ndoctype";
                //case "":
                //    return "normdoc";
                //case "":
                //    return "operstat";
                //case "":
                //    return "room";
                //case "":
                //    return "roomtype";
                //case "":
                //    return "socrbase";
                //case "":
                //    return "stead";
                //case "":
                //    return "strstat";
                default:
                    return "";
            }
        }
        static async Task<bool> ValidateXML(string schemaPath, string xmlPath)
        {
            string xsdMarkup = File.ReadAllText(schemaPath);
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdMarkup)));

            var xmlReader = XmlReader.Create(new FileStream(xmlPath, FileMode.Open, FileAccess.Read), new XmlReaderSettings { Async = true });
            var xmlDocument = await XDocument.LoadAsync(xmlReader, LoadOptions.PreserveWhitespace, CancellationToken.None);

            bool errors = false;
            xmlDocument.Validate(schemas, (obj, eventArguments) =>
            {
                Console.WriteLine("{0}", eventArguments.Message);
                errors = true;
            });
            return !errors;
        }


        static async Task ParseAdrobjXML(string schemaPath, string xmlPath, CancellationToken cancellationToken)
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


        /// <summary>
        /// в одной из колонок значение больше чем максимальное возможное по схеме
        /// value too long for type character varying(10)
        /// 
        /// buildnum   | character varying(10)
        /// strucnum   | character varying(10)
        /// 
        /// </summary>
        /// <param name="schemaPath"></param>
        /// <param name="xmlPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        static async Task ParseHouseXML(string schemaPath, string xmlPath, CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection("Host=localhost;Database=fias;Username=lt;Password=1");
            await connection.OpenAsync(cancellationToken);

            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                using var bar = new ProgressBar(100000, "import house file to database", new ProgressBarOptions {ProgressCharacter = '─', ProgressBarOnBottom = true});
                var skipped = 0;
                var keyField = "houseid";
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

                    // исправление косяков товарищей из nalog.ru
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

                    await using (var cmd = new NpgsqlCommand($"INSERT INTO house ({string.Join(",", insertRow.Keys)})" +
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


        /// <summary>
        /// value too long for type character varying(120)
        ///  number     | character varying(120)
        /// </summary>
        /// <param name="schemaPath"></param>
        /// <param name="xmlPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        static async Task ParseSteadXML(string schemaPath, string xmlPath, CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection("Host=localhost;Database=fias;Username=lt;Password=1");
            await connection.OpenAsync(cancellationToken);

            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                using var bar = new ProgressBar(100000, "import house file to database", new ProgressBarOptions {ProgressCharacter = '─', ProgressBarOnBottom = true});
                var skipped = 0;
                var keyField = "steadid";
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
                        if (attribute.Name.LocalName == "NUMBER" && attribute.Value.Length > 120)
                        {
                            attribute.Value = attribute.Value.Substring(0, 120);
                        }
                        insertRow.Add(attribute.Name.LocalName.ToLower(), attribute.Value.Replace('\'', '\"'));
                    }



                    if (!updateReady)
                    {
                        updateFields = string.Join(",", insertRow.Keys.Where(field => field != keyField));
                        updateValues = string.Join(", EXCLUDED.", insertRow.Keys.Where(field => field != keyField));
                        updateReady = true;
                    }

                    await using (var cmd = new NpgsqlCommand($"INSERT INTO stead " +
                                                             $"({string.Join(",", insertRow.Keys)})" +
                                                             $" VALUES " +
                                                             $"({string.Join(",", insertRow.Values.ToList().Stringify())})" +
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


        static async Task ParseRoomXML(string schemaPath, string xmlPath, CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection("Host=localhost;Database=fias;Username=lt;Password=1");
            await connection.OpenAsync(cancellationToken);

            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                using var bar = new ProgressBar(100000, "import house file to database", new ProgressBarOptions {ProgressCharacter = '─', ProgressBarOnBottom = true});
                var skipped = 0;
                var keyField = "roomid";
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

                    // исправление косяков товарищей из nalog.ru

                    if (!updateReady)
                    {
                        updateFields = string.Join(",", insertRow.Keys.Where(field => field != keyField));
                        updateValues = string.Join(", EXCLUDED.", insertRow.Keys.Where(field => field != keyField));
                        updateReady = true;
                    }

                    await using (var cmd = new NpgsqlCommand($"INSERT INTO room " +
                                                             $"({string.Join(",", insertRow.Keys)})" +
                                                             $" VALUES " +
                                                             $"({string.Join(",", insertRow.Values.ToList().Stringify())})" +
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


        static async Task Main(string[] args)
        {
            fiasContext context = new fiasContext();
            var connection = context.Database.OpenConnectionAsync();

            
            await ParseRoomXML(args[0], args[1], CancellationToken.None);
//            await ParseSteadXML(args[0], args[1], CancellationToken.None);
//            await ParseHouseXML(args[0], args[1], CancellationToken.None);
            //var success = await ValidateXML(args[0], args[1]);
//            await ParseAdrobjXML(args[0], args[1], CancellationToken.None);
        }
    }
}
