using Address.API.Import.Xml.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Address.API.Import.Xml
{
    public class FiasDatabase
    {
        private readonly ILogger<FiasDatabase> _logger;
        private readonly Configuration _configuration;
        private readonly string _connectionString;
        private NpgsqlConnection _connection;

        public FiasDatabase(IOptions<Configuration> config, IConfiguration configuration, ILogger<FiasDatabase> logger)
        {
            _logger = logger;
            _configuration = config.Value;
            _connectionString = configuration.GetConnectionString("fias");
        }

        public bool UpdateRunning { get; private set; }

        private Importer CreateImporter(string xmlPath)
        {
            var tableName = Regex.Match(xmlPath, ".*AS_([A-Z_]+)_\\d{8}_.{8}-.{4}-.{4}-.{4}-.{12}\\.XML", RegexOptions.IgnoreCase).Groups[1].Value;

            return tableName switch
            {
                // update records
                "ADDROBJ" => new Addrobj(_connection, xmlPath, false),
                "HOUSE" => new House(_connection, xmlPath, false),
                "ACTSTAT" => new Actstat(_connection, xmlPath, false),
                "CENTERST" => new Centerst(_connection, xmlPath, false),
                "CURENTST" => new Curentst(_connection, xmlPath, false),
                "ESTSTAT" => new Eststat(_connection, xmlPath, false),
                "FLATTYPE" => new Flattype(_connection, xmlPath, false),
                "NDOCTYPE" => new Ndoctype(_connection, xmlPath, false),
                "NORMDOC" => new Normdoc(_connection, xmlPath, false),
                "OPERSTAT" => new Operstat(_connection, xmlPath, false),
                "ROOM" => new Room(_connection, xmlPath, false),
                "ROOMTYPE" => new Roomtype(_connection, xmlPath, false),
                "SOCRBASE" => new Socrbase(_connection, xmlPath, false),
                "STEAD" => new Stead(_connection, xmlPath, false),
                "STRSTAT" => new Strstat(_connection, xmlPath, false),

                // delete wrong records
                "DEL_ADDROBJ" => new Addrobj(_connection, xmlPath, true),
                "DEL_HOUSE" => new House(_connection, xmlPath, true),
                "DEL_ACTSTAT" => new Actstat(_connection, xmlPath, true),
                "DEL_CENTERST" => new Centerst(_connection, xmlPath, true),
                "DEL_CURENTST" => new Curentst(_connection, xmlPath, true),
                "DEL_ESTSTAT" => new Eststat(_connection, xmlPath, true),
                "DEL_FLATTYPE" => new Flattype(_connection, xmlPath, true),
                "DEL_NDOCTYPE" => new Ndoctype(_connection, xmlPath, true),
                "DEL_NORMDOC" => new Normdoc(_connection, xmlPath, true),
                "DEL_OPERSTAT" => new Operstat(_connection, xmlPath, true),
                "DEL_ROOM" => new Room(_connection, xmlPath, true),
                "DEL_ROOMTYPE" => new Roomtype(_connection, xmlPath, true),
                "DEL_SOCRBASE" => new Socrbase(_connection, xmlPath, true),
                "DEL_STEAD" => new Stead(_connection, xmlPath, true),
                "DEL_STRSTAT" => new Strstat(_connection, xmlPath, true),

                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Update FIAS database using zip archives of incremential changes got from fias.nalog.ru
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Update(CancellationToken cancellationToken)
        {
            try
            {
                UpdateRunning = true;
                _connection = new NpgsqlConnection(_connectionString);

                const string archiveName = "fias_delta_xml.zip";

                var archive = new FileInfo(Path.Combine(_configuration.PathForFiasUpdates, archiveName));
                if (!archive.Exists)
                {
                    _logger.LogError($"Error while updating database: {archive.Name} does not exist");
                    return;
                }
                var unpackedDirectory = new DirectoryInfo(Path.Combine(_configuration.PathForFiasUpdates,
                    Path.GetFileNameWithoutExtension(archiveName)));

                ZipFile.ExtractToDirectory(archive.FullName, unpackedDirectory.FullName, true);

                // import files to database fias
                var xmlFiles = Directory.EnumerateFiles(unpackedDirectory.FullName);

                foreach (var xmlFile in xmlFiles)
                {
                    await CreateImporter(xmlFile).Run(cancellationToken);
                }

                // update version of database in settings table
                // "AS_ACTSTAT_20200514_202dbf49-3ddd-4f13-89ac-849ea067d6fa.XML"
                var updateDateString = Regex.Match(xmlFiles.First(), "AS_.+_([0-9]{8})_.+\\.XML").Groups[1]?.Value;
                //var updateDate = DateTime.ParseExact(updateDateString,"yyyyMMdd", null);
                await SetSettingsAsync("updateDate", updateDateString);

                // clean bullshit after processing and delete archive
                unpackedDirectory.Delete(recursive: true);
                archive.Delete();
            }
            catch (Exception exception)
            {
                _logger.LogError("Error while updating database:", exception);
            }
            finally
            {
                UpdateRunning = false;
                _logger.LogInformation("Update FIAS database finished.");
            }
        }

        public async Task SetSettingsAsync(string key, string value)
        {
            try
            {
                await _connection.OpenAsync(CancellationToken.None);
                await using var cmd = new NpgsqlCommand($"UPDATE settings SET value = '{value}' WHERE key = '{key}'", _connection);
                await cmd.ExecuteNonQueryAsync(CancellationToken.None);
                await _connection.CloseAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to set update date for database.");
            }
        }


        /// <summary>
        /// Initialize empty FIAS database from fias_xml.zip file which is dumped to xml database with addresses.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Initialize(CancellationToken cancellationToken)
        {
            var fiasDatabaseArchive = "fias_xml.zip";
            var files = Directory.EnumerateFiles(_configuration.PathForFiasUpdates, fiasDatabaseArchive, SearchOption.AllDirectories).ToList();
            if (!files.Any())
            {
                throw new Exception("Unable to find archive with xml files of FIAS database");
            }

            fiasDatabaseArchive = files.First();
            var unpackedDirectory = new DirectoryInfo(Path.Combine(_configuration.PathForFiasUpdates, Path.GetFileNameWithoutExtension(fiasDatabaseArchive)));
            ZipFile.ExtractToDirectory(fiasDatabaseArchive, unpackedDirectory.FullName);
            
            var xmlFiles = Directory.EnumerateFiles(unpackedDirectory.FullName);

            foreach (var xmlFile in xmlFiles)
            {
                await CreateImporter(xmlFile).Run(cancellationToken);
            }

            // clean bullshit after processing
            unpackedDirectory.Delete();
        }
    }
}
