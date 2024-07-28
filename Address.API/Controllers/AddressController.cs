using Address.API.Common;
using Address.API.Import.Xml;
using Address.API.Model;
using Address.API.Request;
using CommonExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Address.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    [Route("api/v{version}/[controller]/")]
    public class AddressController : Controller
    {
        private readonly ILogger<AddressController> _logger;
        private readonly Configuration _config;
        private readonly fiasContext _context;
        private readonly FiasDatabase _fiasDatabase;

        public AddressController(ILogger<AddressController> logger, IOptions<Configuration> config, fiasContext context, FiasDatabase fiasDatabase)
        {
            _logger = logger;
            _config = config.Value;
            _context = context;
            _fiasDatabase = fiasDatabase;
        }

        /*
         * ACTSTATUS - Статус актуальности адресного объекта ФИАС.
         * Принимает значения: 1 - актуальный, 0 – не актуальный (см. табл. ACTSTAT).
         * Отвечает непосредственно за актуальность "имени". Если объект был переименован (чаще это исправление опечаток),
         * то старая запись получает CURRSTATUS=1 и ACTSTATUS=0. Если же административная единица была ликвидирована или переподчинена,
         * то имя останется по-прежнему актуальным: CURRSTATUS=99/51 и ACTSTATUS=1. В тоже время, при внесении изменений, не касающихся
         * непосредственно адресной части, признак актуальности все равно сбрасывается (ACTSTATUS=0).
         * источник - https://wiki.gis-lab.info/w/%D0%A4%D0%98%D0%90%D0%A1#.D0.A1.D1.82.D0.B0.D1.82.D1.83.D1.81_.D0.B0.D0.BA.D1.82.D1.83.D0.B0.D0.BB.D1.8C.D0.BD.D0.BE.D1.81.D1.82.D0.B8
         *
         */


        /// <summary>
        /// Find region by pattern
        /// </summary>
        /// <param name="addressObjectId"></param>
        /// <returns></returns>
        [HttpGet("getAddressObject")]
        [AllowAnonymous]
        public async Task<ActionResult> GetFullAddressObjectById([FromQuery] string addressObjectId)
        {
            _logger.LogInformation($"getting address object by id: {addressObjectId}");
            try
            {
                if (string.IsNullOrEmpty(addressObjectId))
                {
                    return this.Status422UnprocessableEntity(message: "422", logger: _logger,
                        localizer: null,
                        level: LogLevel.Error);
                }

                var localities = await _context.Addrobj.FromSqlRaw(
                    "SELECT *" +
                    "    FROM addrobj a " +
                    "WHERE " +
                    "  a.actstatus=1" +
                    "  AND a.aoguid = @addressobject " +
                    "ORDER BY a.startdate DESC",
                    new NpgsqlParameter("@addressobject", addressObjectId))
                    .Select(addressObject => addressObject.Aoguid).ToListAsync();

/*
                var localities = await _context.Addrobj
                    .Where(ao => ao.Actstatus == 1 && ao.Aoguid == addressObjectId)
                    .OrderByDescending(ao => ao.Startdate)
                    .Select(addressObject => addressObject.Aoguid)
                    .ToListAsync();
*/
                var localityId = localities.First();

                var fullLocality = await _context.Addrobj.FromSqlRaw(
                    "WITH RECURSIVE parent AS" +
                    " ( SELECT a.*" +
                    "   FROM addrobj a" +
                    "   WHERE a.aoguid = @localityid" +
                    "     AND a.actstatus = 1" +
                    "   UNION ALL" +
                    "   SELECT ao.*" +
                    "   FROM parent p" +
                    "   JOIN addrobj ao ON p.parentguid = ao.aoguid" +
                    "   WHERE p.aoguid IS NOT NULL" +
                    "     AND ao.actstatus = 1) " +
                    "SELECT * " +
                    "FROM parent ", new NpgsqlParameter("@localityid", localityId)).ToListAsync();

                var foundAddressObject = fullLocality.First();

                return Json(new Region
                {
                    Name = string.Join(", ",
                        fullLocality.Select(address => $"{address.Shortname}.{address.Formalname}")),
                    Id = foundAddressObject.Aoguid,
                    Okato = foundAddressObject.Okato,
                    AddressObjectType = foundAddressObject.Aolevel,
                    RegionalCenter = Regex.Match(foundAddressObject.Okato, "[0-9]{2}401000000").Success
                });
            }
            catch (Exception exception)
            {
                return this.Status500InternalServerError(message: exception.Message, logger: _logger,
                    localizer: null,
                    level: LogLevel.Error);
            }
        }

        /// <summary>
        /// Find region by pattern
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("region")]
        [AllowAnonymous]
        public async Task<ActionResult> FindRegion([FromBody] RegionRequest request )
        {
            _logger.LogInformation($"Getting full name of region object: {JsonSerializer.Serialize(request)}");
            try
            {
                var localities = await _context.Addrobj.FromSqlRaw(
                    "WITH addrobj_noduplicates AS " +
                    "(" +
                    "       SELECT *, ROW_NUMBER() OVER (PARTITION BY fa.aoguid ORDER BY fa.startdate DESC) as rn " +
                    "       FROM fias.addrobj fa " +
                    "       WHERE " +
                    "           fa.aolevel = ANY (:aolevel)" +
                    "           AND fa.actstatus=1" +
                    "           AND fa.formalname ~* @region" +
                    "       ORDER BY fa.aolevel, fa.startdate DESC" +
                    "       LIMIT @limit" +
                    ") " +
                    "SELECT *" +
                    "    FROM addrobj_noduplicates fa " +
                    "WHERE" +
                    "   fa.rn = 1",
                    new NpgsqlParameter("@region", string.IsNullOrEmpty(request.RegionSearch) ? ".*" : request.RegionSearch),
                    new NpgsqlParameter("aolevel", NpgsqlDbType.Array | NpgsqlDbType.Integer, request.AddressObjectTypes.Length)
                    {
                        Value = request.AddressObjectTypes 
                    },
                    new NpgsqlParameter("@limit", request.Limit) )
                    .Select(addressObject => addressObject.Aoguid)
                    .ToListAsync();

/*
                // В 10 РАЗ МЕДЛЕННЕЕ !!! НЕ ИСПОЛЬЗОВАТЬ LINQ!!!
                var localities = await _context.Addrobj
                    .Where(ao =>
                            request.AddressObjectTypes.Contains(ao.Aolevel) 
                            && ao.Actstatus == 1
                            && (string.IsNullOrEmpty(request.RegionSearch) || ao.Formalname.ToLower().Contains(request.RegionSearch.ToLower()))
                            )
                    .OrderBy(ao => ao.Startdate)
                    .OrderBy(ao => ao.Aolevel)
                    .Select(addressObject => addressObject.Aoguid)
                    .ToListAsync();
*/


                if ( ! string.IsNullOrEmpty(request.AddressObjectId) && localities.All(addressObjectId => addressObjectId != request.AddressObjectId))
                {
                    localities.Insert(0,request.AddressObjectId);
                }

                var result = localities.Select(async localityId =>
                {
                    var fullLocality = await _context.Addrobj.FromSqlRaw(
                        "WITH RECURSIVE parent AS" +
                        " ( SELECT a.*" +
                        "   FROM addrobj a" +
                        "   WHERE a.aoguid = @locality" +
                        "     AND a.actstatus = 1" +
                        "   UNION ALL" +
                        "   SELECT ao.*" +
                        "   FROM parent p" +
                        "   JOIN addrobj ao ON p.parentguid = ao.aoguid" +
                        "   WHERE p.aoguid IS NOT NULL" +
                        "     AND ao.actstatus = 1) " +
                        "SELECT * " +
                        "FROM parent ", new NpgsqlParameter("@locality", localityId)).ToListAsync();

                    if (fullLocality.Count == 0)
                    {
                        return new Region {Id = localityId};
                    }

                    var foundRegion = fullLocality.First();
                    return new Region
                    {
                        Name = string.Join(", ",
                            fullLocality.Select(address => $"{address.Shortname}.{address.Formalname}")),
                        Id = foundRegion.Aoguid,
                        Okato = foundRegion.Okato,
                        AddressObjectType = foundRegion.Aolevel,
                        RegionalCenter = Regex.Match(foundRegion.Okato, "[0-9]{2}401000000").Success
                    };
                }).Select(taskRegion => taskRegion.Result).ToList();

                return Json(result);
            }
            catch (Exception exception)
            {
                return this.Status500InternalServerError(message: exception.Message, logger: _logger,
                    localizer: null,
                    level: LogLevel.Error);
            }
        }

        /// <summary>
        /// Find street by pattern
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="streetSearch"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("street")]
        [AllowAnonymous]
        public async Task<ActionResult> FindStreet(
            [FromQuery][Required] Guid regionId,
            [FromQuery] string streetSearch,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation($"request address: {streetSearch}");
            try
            {
               var result = await _context.Addrobj.FromSqlRaw(
                    "SELECT * " +
                    "FROM addrobj a " +
                    "WHERE a.aolevel = 7" +
                    "  AND a.actstatus = 1" +
                    "  AND LOWER(a.formalname) ~* @street" +
//                    "  AND a.parentguid = @region " +
                    "  AND a.parentguid = @region " +
                    "ORDER BY a.formalname " +
                    "LIMIT @limit",
                    new NpgsqlParameter("@street", string.IsNullOrEmpty(streetSearch) ? ".*" : streetSearch), 
                    new NpgsqlParameter("@region", regionId.ToString()),
                    new NpgsqlParameter("@limit", limit))
                    .Select(a => new Street{Name = $"{a.Shortname}.{a.Formalname}", Id = a.Aoguid}).ToListAsync();

/*
                var result = await _context.Addrobj
                    .Where(ao => ao.Aolevel == 7 && ao.Actstatus == 1 && (string.IsNullOrEmpty(streetSearch) || ao.Formalname.ToLower().Contains(streetSearch.ToLower())) && regionId.ToString() == ao.Parentguid)
                    .OrderBy( street => street.Formalname)
                    .Select(street => new Street {Name = $"{street.Shortname}.{street.Formalname}", Id = street.Aoguid})
                    .Take(limit)
                    .ToListAsync();
*/
                return Json(result );
            }
            catch (Exception exception)
            {
                return this.Status500InternalServerError(message: exception.Message, logger: _logger,
                    localizer: null,
                    level: LogLevel.Error);
            }
        }

        [HttpGet("house")]
        [AllowAnonymous]
        public async Task<ActionResult> FindHouse(
            [FromQuery][Required] Guid streetId,
            [FromQuery] string houseNumber,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation($"request house number: {houseNumber} and street: {streetId}");
            try
            {

/*                
                var result = await _context.House.FromSqlRaw(
                    "SELECT * " +
                    "FROM house h " +
                    "WHERE h.aoguid = @street " +
                    "  AND h.housenum ~* @house " + 
                    "ORDER BY h.housenum " +
                    "LIMIT @limit",
                    new NpgsqlParameter("@street", streetId.ToString()), 
                    new NpgsqlParameter("@house", string.IsNullOrEmpty(houseNumber) ? ".*" : houseNumber),
                    new NpgsqlParameter("@limit", limit))
                    .Select(house => new HouseNumber{Number = house.Housenum}).ToListAsync();

                return Json(result);
*/

/*
                result = await _context.HouseNumber.FromSqlRaw(
                    "SELECT * " +
                    "FROM fias.house h " +
                    "  JOIN fias.strstat s ON s.strstatid = h.strstatus" +
                    "  JOIN fias.eststat e ON e.eststatid = h.eststatus" +
                    "WHERE h.aoguid = {0} " +
                    "AND h.housenum ~* {1}", streetId, houseNumber).Select(house => new HouseNumber{Number = house.Housenum}).ToListAsync();

*/



                return Json(await _context.House
                                    .Where(house => house.Aoguid == streetId.ToString() && house.Housenum.Contains(houseNumber))
                                    .OrderBy(house => house.Housenum)
                                    .Take(limit)
                                    .Select(house => new HouseNumber{Number = house.Housenum})
                                    .ToArrayAsync()
                );
            }
            catch (Exception exception)
            {
                return this.Status500InternalServerError(message: exception.Message, logger: _logger,
                    localizer: null,
                    level: LogLevel.Error);
            }
        }

        /// <summary>
        /// Update FIAS database
        /// Path where files have to be located before run = config.PathForFiasUpdates
        /// </summary>
        /// <returns></returns>
        [HttpGet("update")]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateFromFiasZip()
        {
            _logger.LogInformation($"Starting update database from FIAS zip archive");
            try
            {
                if (_fiasDatabase.UpdateRunning)
                {
                    _logger.LogInformation($"Update database is in progress. You can run next update when current update will finish");
                    return this.Status200Ok(message: Messages.UpdateRunning, logger:_logger);
                }

                await Task.Factory.StartNew(async delegate { await _fiasDatabase.Update(CancellationToken.None); });

                return this.Status200Ok(message: Messages.UpdateStarted, logger:_logger);
            }
            catch (Exception exception)
            {
                return this.Status500InternalServerError(localizer:null, message: exception.Message, logger: _logger, level: LogLevel.Error);
            }
        }

        /// <summary>
        /// Initialize FIAS database
        /// Path where files have to be located before run = config.PathForFiasUpdates
        /// </summary>
        /// <returns></returns>
        [HttpGet("initialize")]
        [AllowAnonymous]
        public async Task<ActionResult> InitializeFromFiasZip()
        {
            _logger.LogInformation($"Starting initialize database from FIAS zip archive");
            try
            {
                if (_fiasDatabase.UpdateRunning)
                {
                    _logger.LogInformation($"Initialize database is in progress. You can run next update when current update will finish");
                    return this.Status200Ok(message:Messages.UpdateRunning, logger:_logger);
                }

                await Task.Factory.StartNew(async delegate { await _fiasDatabase.Initialize(CancellationToken.None); });

                return this.Status200Ok(message:Messages.UpdateStarted, logger:_logger);
            }
            catch (Exception exception)
            {
                return this.Status500InternalServerError(localizer:null, message: exception.Message, logger: _logger,
                    level: LogLevel.Error);
            }
        }
    }
}
