using Microsoft.EntityFrameworkCore;

namespace FIAS
{
    public partial class fiasContext : DbContext
    {
        public fiasContext()
        {
        }

        public fiasContext(DbContextOptions<fiasContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Actstat> Actstat { get; set; }
        public virtual DbSet<Addrobj> Addrobj { get; set; }
        public virtual DbSet<Centerst> Centerst { get; set; }
        public virtual DbSet<Curentst> Curentst { get; set; }
        public virtual DbSet<Eststat> Eststat { get; set; }
        public virtual DbSet<Flattype> Flattype { get; set; }
        public virtual DbSet<House> House { get; set; }
        public virtual DbSet<Ndoctype> Ndoctype { get; set; }
        public virtual DbSet<Normdoc> Normdoc { get; set; }
        public virtual DbSet<Operstat> Operstat { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<Roomtype> Roomtype { get; set; }
        public virtual DbSet<Socrbase> Socrbase { get; set; }
        public virtual DbSet<Stead> Stead { get; set; }
        public virtual DbSet<Strstat> Strstat { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=fias;Username=lt;Password=1");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum(null, "enum_house_divtype", new[] { "0", "1", "2" })
                .HasPostgresEnum(null, "enum_object_divtype", new[] { "0", "1", "2" })
                .HasPostgresEnum(null, "enum_object_livestatus", new[] { "0", "1" })
                .HasPostgresEnum(null, "enum_room_livestatus", new[] { "0", "1" })
                .HasPostgresEnum(null, "enum_stead_divtype", new[] { "0", "1", "2" })
                .HasPostgresEnum(null, "enum_stead_livestatus", new[] { "0", "1" });

            modelBuilder.Entity<Actstat>(entity =>
            {
                entity.ToTable("actstat", "fias");

                entity.HasComment("Состав и структура файла с информацией по статусу актуальности в БД ФИАС");

                entity.Property(e => e.Actstatid)
                    .HasColumnName("actstatid")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Addrobj>(entity =>
            {
                entity.HasKey(e => e.Aoid)
                    .HasName("idx_18615_primary");

                entity.ToTable("addrobj", "fias");

                entity.HasComment("Состав и структура файла с информацией классификатора адресообразующих элементов БД ФИАС");

                entity.HasIndex(e => e.Actstatus)
                    .HasName("idx_18615_actstatus");

                entity.HasIndex(e => e.Aolevel)
                    .HasName("idx_18615_aolevel");

                entity.HasIndex(e => e.Centstatus)
                    .HasName("idx_18615_centstatus");

                entity.HasIndex(e => e.Currstatus)
                    .HasName("idx_18615_currstatus");

                entity.HasIndex(e => e.Normdoc)
                    .HasName("idx_18615_normdoc");

                entity.HasIndex(e => e.Operstatus)
                    .HasName("idx_18615_operstatus");

                entity.Property(e => e.Aoid)
                    .HasColumnName("aoid")
                    .HasMaxLength(36)
                    .HasComment("Уникальный идентификатор записи. Ключевое поле.");

                entity.Property(e => e.Actstatus)
                    .HasColumnName("actstatus")
                    .HasComment("Статус актуальности адресного объекта ФИАС. Актуальный адрес на текущую дату. Обычно последняя запись об адресном объекте.");

                entity.Property(e => e.Aoguid)
                    .IsRequired()
                    .HasColumnName("aoguid")
                    .HasMaxLength(36)
                    .HasComment("Глобальный уникальный идентификатор адресного объекта ");

                entity.Property(e => e.Aolevel)
                    .HasColumnName("aolevel")
                    .HasComment("Уровень адресного объекта ");

                entity.Property(e => e.Areacode)
                    .IsRequired()
                    .HasColumnName("areacode")
                    .HasMaxLength(3)
                    .HasComment("Код района");

                entity.Property(e => e.Autocode)
                    .IsRequired()
                    .HasColumnName("autocode")
                    .HasMaxLength(1)
                    .HasComment("Код автономии");

                entity.Property(e => e.Centstatus)
                    .HasColumnName("centstatus")
                    .HasComment("Статус центра");

                entity.Property(e => e.Citycode)
                    .IsRequired()
                    .HasColumnName("citycode")
                    .HasMaxLength(3)
                    .HasComment("Код города");

                entity.Property(e => e.Code)
                    .HasColumnName("code")
                    .HasMaxLength(17)
                    .HasComment("Код адресного объекта одной строкой с признаком актуальности из КЛАДР 4.0. ");

                entity.Property(e => e.Ctarcode)
                    .IsRequired()
                    .HasColumnName("ctarcode")
                    .HasMaxLength(3)
                    .HasComment("Код внутригородского района");

                entity.Property(e => e.Currstatus)
                    .HasColumnName("currstatus")
                    .HasComment("Статус актуальности КЛАДР 4 (последние две цифры в коде)");

                entity.Property(e => e.Divtype).HasColumnName("divtype");

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date")
                    .HasComment("Окончание действия записи");

                entity.Property(e => e.Extrcode)
                    .IsRequired()
                    .HasColumnName("extrcode")
                    .HasMaxLength(4)
                    .HasComment("Код дополнительного адресообразующего элемента");

                entity.Property(e => e.Formalname)
                    .IsRequired()
                    .HasColumnName("formalname")
                    .HasMaxLength(120)
                    .HasComment("Формализованное наименование");

                entity.Property(e => e.Ifnsfl)
                    .HasColumnName("ifnsfl")
                    .HasMaxLength(4)
                    .HasComment("Код ИФНС ФЛ");

                entity.Property(e => e.Ifnsul)
                    .HasColumnName("ifnsul")
                    .HasMaxLength(4)
                    .HasComment("Код ИФНС ЮЛ");

                entity.Property(e => e.Livestatus)
                    .HasColumnName("livestatus")
                    .HasComment("Признак действующего адресного объекта");

                entity.Property(e => e.Nextid)
                    .HasColumnName("nextid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор записи  связывания с последующей исторической записью");

                entity.Property(e => e.Normdoc)
                    .HasColumnName("normdoc")
                    .HasMaxLength(36)
                    .HasComment("Внешний ключ на нормативный документ");

                entity.Property(e => e.Offname)
                    .HasColumnName("offname")
                    .HasMaxLength(120)
                    .HasComment("Официальное наименование");

                entity.Property(e => e.Okato)
                    .HasColumnName("okato")
                    .HasMaxLength(11)
                    .HasComment("OKATO");

                entity.Property(e => e.Oktmo)
                    .HasColumnName("oktmo")
                    .HasMaxLength(11)
                    .HasComment("OKTMO");

                entity.Property(e => e.Operstatus)
                    .HasColumnName("operstatus")
                    .HasComment("Статус действия над записью – причина появления записи (см. описание таблицы OperationStatus):");

                entity.Property(e => e.Parentguid)
                    .HasColumnName("parentguid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор объекта родительского объекта");

                entity.Property(e => e.Placecode)
                    .IsRequired()
                    .HasColumnName("placecode")
                    .HasMaxLength(3)
                    .HasComment("Код населенного пункта");

                entity.Property(e => e.Plaincode)
                    .HasColumnName("plaincode")
                    .HasMaxLength(15)
                    .HasComment("Код адресного объекта из КЛАДР 4.0 одной строкой без признака актуальности (последних двух цифр)");

                entity.Property(e => e.Plancode)
                    .IsRequired()
                    .HasColumnName("plancode")
                    .HasMaxLength(4)
                    .HasComment("Код элемента планировочной структуры");

                entity.Property(e => e.Postalcode)
                    .HasColumnName("postalcode")
                    .HasMaxLength(6)
                    .HasComment("Почтовый индекс");

                entity.Property(e => e.Previd)
                    .HasColumnName("previd")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор записи связывания с предыдушей исторической записью");

                entity.Property(e => e.Regioncode)
                    .IsRequired()
                    .HasColumnName("regioncode")
                    .HasMaxLength(2)
                    .HasComment("Код региона");

                entity.Property(e => e.Sextcode)
                    .IsRequired()
                    .HasColumnName("sextcode")
                    .HasMaxLength(3)
                    .HasComment("Код подчиненного дополнительного адресообразующего элемента");

                entity.Property(e => e.Shortname)
                    .IsRequired()
                    .HasColumnName("shortname")
                    .HasMaxLength(10)
                    .HasComment("Краткое наименование типа объекта");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date")
                    .HasComment("Начало действия записи");

                entity.Property(e => e.Streetcode)
                    .HasColumnName("streetcode")
                    .HasMaxLength(4)
                    .HasComment("Код улицы");

                entity.Property(e => e.Terrifnsfl)
                    .HasColumnName("terrifnsfl")
                    .HasMaxLength(4)
                    .HasComment("Код территориального участка ИФНС ФЛ");

                entity.Property(e => e.Terrifnsul)
                    .HasColumnName("terrifnsul")
                    .HasMaxLength(4)
                    .HasComment("Код территориального участка ИФНС ЮЛ");

                entity.Property(e => e.Updatedate)
                    .HasColumnName("updatedate")
                    .HasColumnType("date")
                    .HasComment("Дата  внесения записи");
            });

            modelBuilder.Entity<Centerst>(entity =>
            {
                entity.ToTable("centerst", "fias");

                entity.HasComment("Состав и структура файла с информацией по статусу центра в БД ФИАС");

                entity.Property(e => e.Centerstid)
                    .HasColumnName("centerstid")
                    .HasComment("Идентификатор статуса")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .HasComment("Наименование");
            });

            modelBuilder.Entity<Curentst>(entity =>
            {
                entity.ToTable("curentst", "fias");

                entity.HasComment("Состав и структура файла с информацией по статусу актуальности КЛАДР 4.0 в БД ФИАС");

                entity.Property(e => e.Curentstid)
                    .HasColumnName("curentstid")
                    .HasComment("Идентификатор статуса (ключ)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .HasComment("Наименование (0 - актуальный, 1-50, 2-98 – исторический (кроме 51), 51 - переподчиненный, 99 - несуществующий)");
            });

            modelBuilder.Entity<Eststat>(entity =>
            {
                entity.ToTable("eststat", "fias");

                entity.HasComment("Состав и структура файла с информацией по признакам владения в БД ФИАС");

                entity.Property(e => e.Eststatid)
                    .HasColumnName("eststatid")
                    .HasComment("Признак владения")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20)
                    .HasComment("Наименование");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname")
                    .HasMaxLength(20)
                    .HasComment("Краткое наименование");
            });

            modelBuilder.Entity<Flattype>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("flattype", "fias");

                entity.HasComment("Состав и структура файла с информацией по типам помещений в БД ФИАС");

                entity.Property(e => e.Fltypeid)
                    .HasColumnName("fltypeid")
                    .HasComment("Тип помещения");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20)
                    .HasComment("Наименование");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname")
                    .HasMaxLength(20)
                    .HasComment("Краткое наименование");
            });

            modelBuilder.Entity<House>(entity =>
            {
                entity.ToTable("house", "fias");

                entity.HasComment("Состав и структура файла с информацией по номерам домов улиц городов и населенных пунктов в БД ФИАС");

                entity.HasIndex(e => e.Eststatus)
                    .HasName("idx_18633_eststatus");

                entity.HasIndex(e => e.Normdoc)
                    .HasName("idx_18633_normdoc");

                entity.HasIndex(e => e.Statstatus)
                    .HasName("idx_18633_statstatus");

                entity.HasIndex(e => e.Strstatus)
                    .HasName("idx_18633_strstatus");

                entity.Property(e => e.Houseid)
                    .HasColumnName("houseid")
                    .HasMaxLength(36)
                    .HasComment("Уникальный идентификатор записи дома");

                entity.Property(e => e.Aoguid)
                    .IsRequired()
                    .HasColumnName("aoguid")
                    .HasMaxLength(36)
                    .HasComment("Guid записи родительского объекта (улицы, города, населенного пункта и т.п.)");

                entity.Property(e => e.Buildnum)
                    .HasColumnName("buildnum")
                    .HasMaxLength(10)
                    .HasComment("Номер корпуса");

                entity.Property(e => e.Cadnum)
                    .HasColumnName("cadnum")
                    .HasMaxLength(100)
                    .HasComment("Кадастровый номер");

                entity.Property(e => e.Counter)
                    .HasColumnName("counter")
                    .HasComment("Счетчик записей домов для КЛАДР 4");

                entity.Property(e => e.Divtype)
                    .HasColumnName("divtype")
                    .HasComment("Тип адресации:");

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date")
                    .HasComment("Окончание действия записи");

                entity.Property(e => e.Eststatus)
                    .HasColumnName("eststatus")
                    .HasComment("Признак владения");

                entity.Property(e => e.Houseguid)
                    .IsRequired()
                    .HasColumnName("houseguid")
                    .HasMaxLength(36)
                    .HasComment("Глобальный уникальный идентификатор дома");

                entity.Property(e => e.Housenum)
                    .HasColumnName("housenum")
                    .HasMaxLength(20)
                    .HasComment("Номер дома");

                entity.Property(e => e.Ifnsfl)
                    .HasColumnName("ifnsfl")
                    .HasMaxLength(4)
                    .HasComment("Код ИФНС ФЛ");

                entity.Property(e => e.Ifnsul)
                    .HasColumnName("ifnsul")
                    .HasMaxLength(4)
                    .HasComment("Код ИФНС ЮЛ");

                entity.Property(e => e.Normdoc)
                    .HasColumnName("normdoc")
                    .HasMaxLength(36)
                    .HasComment("Внешний ключ на нормативный документ");

                entity.Property(e => e.Okato)
                    .HasColumnName("okato")
                    .HasMaxLength(11)
                    .HasComment("OKATO");

                entity.Property(e => e.Oktmo)
                    .HasColumnName("oktmo")
                    .HasMaxLength(11)
                    .HasComment("OKTMO");

                entity.Property(e => e.Postalcode)
                    .HasColumnName("postalcode")
                    .HasMaxLength(6)
                    .HasComment("Почтовый индекс");

                entity.Property(e => e.Regioncode)
                    .HasColumnName("regioncode")
                    .HasMaxLength(2)
                    .HasComment("Код региона");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date")
                    .HasComment("Начало действия записи");

                entity.Property(e => e.Statstatus)
                    .HasColumnName("statstatus")
                    .HasComment("Состояние дома");

                entity.Property(e => e.Strstatus)
                    .HasColumnName("strstatus")
                    .HasComment("Признак строения");

                entity.Property(e => e.Strucnum)
                    .HasColumnName("strucnum")
                    .HasMaxLength(10)
                    .HasComment("Номер строения");

                entity.Property(e => e.Terrifnsfl)
                    .HasColumnName("terrifnsfl")
                    .HasMaxLength(4)
                    .HasComment("Код территориального участка ИФНС ФЛ");

                entity.Property(e => e.Terrifnsul)
                    .HasColumnName("terrifnsul")
                    .HasMaxLength(4)
                    .HasComment("Код территориального участка ИФНС ЮЛ");

                entity.Property(e => e.Updatedate)
                    .HasColumnName("updatedate")
                    .HasColumnType("date")
                    .HasComment("Дата время внесения записи");
            });

            modelBuilder.Entity<Ndoctype>(entity =>
            {
                entity.HasKey(e => e.Ndtypeid)
                    .HasName("idx_18636_primary");

                entity.ToTable("ndoctype", "fias");

                entity.HasComment("Состав и структура файла с информацией по типу нормативного документа в БД ФИАС");

                entity.Property(e => e.Ndtypeid)
                    .HasColumnName("ndtypeid")
                    .HasComment("Идентификатор записи (ключ)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(250)
                    .HasComment("Наименование типа нормативного документа");
            });

            modelBuilder.Entity<Normdoc>(entity =>
            {
                entity.ToTable("normdoc", "fias");

                entity.HasComment("Состав и структура файла с информацией по сведениям по нормативным документам, являющимся основанием присвоения адресному элементу наименования в БД ФИАС");

                entity.HasIndex(e => e.Doctype)
                    .HasName("idx_18639_doctype");

                entity.Property(e => e.Normdocid)
                    .HasColumnName("normdocid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор нормативного документа");

                entity.Property(e => e.Docdate)
                    .HasColumnName("docdate")
                    .HasColumnType("date")
                    .HasComment("Дата документа");

                entity.Property(e => e.Docimgid)
                    .HasColumnName("docimgid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор образа (внешний ключ)");

                entity.Property(e => e.Docname)
                    .HasColumnName("docname")
                    .HasMaxLength(128)
                    .HasComment("Наименование документа");

                entity.Property(e => e.Docnum)
                    .HasColumnName("docnum")
                    .HasMaxLength(20)
                    .HasComment("Номер документа");

                entity.Property(e => e.Doctype)
                    .HasColumnName("doctype")
                    .HasComment("Тип документа");
            });

            modelBuilder.Entity<Operstat>(entity =>
            {
                entity.ToTable("operstat", "fias");

                entity.HasComment("Состав и структура файла с информацией по статусу действия в БД ФИАС");

                entity.Property(e => e.Operstatid)
                    .HasColumnName("operstatid")
                    .HasComment("Идентификатор статуса (ключ)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .HasComment("Наименование");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("room", "fias");

                entity.HasComment("Состав и структура файла со сведениями о помещениях");

                entity.Property(e => e.Cadnum)
                    .HasColumnName("cadnum")
                    .HasMaxLength(100)
                    .HasComment("Кадастровый номер помещения");

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date")
                    .HasComment("Окончание действия записи");

                entity.Property(e => e.Flatnumber)
                    .IsRequired()
                    .HasColumnName("flatnumber")
                    .HasMaxLength(50)
                    .HasComment("Номер помещения или офиса");

                entity.Property(e => e.Flattype)
                    .HasColumnName("flattype")
                    .HasComment("Тип помещения");

                entity.Property(e => e.Houseguid)
                    .IsRequired()
                    .HasColumnName("houseguid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор родительского объекта (дома)");

                entity.Property(e => e.Livestatus)
                    .HasColumnName("livestatus")
                    .HasComment("Признак действующего адресного объекта");

                entity.Property(e => e.Nextid)
                    .HasColumnName("nextid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор записи  связывания с последующей исторической записью");

                entity.Property(e => e.Normdoc)
                    .HasColumnName("normdoc")
                    .HasMaxLength(36)
                    .HasComment("Внешний ключ на нормативный документ");

                entity.Property(e => e.Operstatus)
                    .HasColumnName("operstatus")
                    .HasComment("Статус действия над записью – причина появления записи (см. описание таблицы OperationStatus):");

                entity.Property(e => e.Postalcode)
                    .HasColumnName("postalcode")
                    .HasMaxLength(6)
                    .HasComment("Почтовый индекс");

                entity.Property(e => e.Previd)
                    .HasColumnName("previd")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор записи связывания с предыдушей исторической записью");

                entity.Property(e => e.Regioncode)
                    .IsRequired()
                    .HasColumnName("regioncode")
                    .HasMaxLength(2)
                    .HasComment("Код региона");

                entity.Property(e => e.Roomcadnum)
                    .HasColumnName("roomcadnum")
                    .HasMaxLength(100)
                    .HasComment("Кадастровый номер комнаты в помещении");

                entity.Property(e => e.Roomguid)
                    .IsRequired()
                    .HasColumnName("roomguid")
                    .HasMaxLength(36)
                    .HasComment("Глобальный уникальный идентификатор адресного объекта (помещения)");

                entity.Property(e => e.Roomid)
                    .IsRequired()
                    .HasColumnName("roomid")
                    .HasMaxLength(36)
                    .HasComment("Уникальный идентификатор записи. Ключевое поле.");

                entity.Property(e => e.Roomnumber)
                    .HasColumnName("roomnumber")
                    .HasMaxLength(50)
                    .HasComment("Номер комнаты");

                entity.Property(e => e.Roomtype)
                    .HasColumnName("roomtype")
                    .HasComment("Тип комнаты");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date")
                    .HasComment("Начало действия записи");

                entity.Property(e => e.Updatedate)
                    .HasColumnName("updatedate")
                    .HasColumnType("date")
                    .HasComment("Дата  внесения записи");
            });

            modelBuilder.Entity<Roomtype>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("roomtype", "fias");

                entity.HasComment("Состав и структура файла с информацией по типам комнат в БД ФИАС");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20)
                    .HasComment("Наименование");

                entity.Property(e => e.Rmtypeid)
                    .HasColumnName("rmtypeid")
                    .HasComment("Тип комнаты");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname")
                    .HasMaxLength(20)
                    .HasComment("Краткое наименование");
            });

            modelBuilder.Entity<Socrbase>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("socrbase", "fias");

                entity.HasComment("Состав и структура файла с информацией по типам адресных объектов в БД ФИАС");

                entity.HasIndex(e => e.Scname)
                    .HasName("idx_18654_scname");

                entity.Property(e => e.KodTSt)
                    .IsRequired()
                    .HasColumnName("kod_t_st")
                    .HasMaxLength(4)
                    .HasComment("Ключевое поле");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasComment("Уровень адресного объекта");

                entity.Property(e => e.Scname)
                    .HasColumnName("scname")
                    .HasMaxLength(10)
                    .HasComment("Краткое наименование типа объекта");

                entity.Property(e => e.Socrname)
                    .IsRequired()
                    .HasColumnName("socrname")
                    .HasMaxLength(50)
                    .HasComment("Полное наименование типа объекта");
            });

            modelBuilder.Entity<Stead>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("stead", "fias");

                entity.HasComment("Состав и структура файла со сведениями о земельных участках");

                entity.Property(e => e.Cadnum)
                    .HasColumnName("cadnum")
                    .HasMaxLength(100)
                    .HasComment("Кадастровый номер");

                entity.Property(e => e.Divtype)
                    .HasColumnName("divtype")
                    .HasComment("Тип адресации:");

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date")
                    .HasComment("Окончание действия записи");

                entity.Property(e => e.Ifnsfl)
                    .HasColumnName("ifnsfl")
                    .HasMaxLength(4)
                    .HasComment("Код ИФНС ФЛ");

                entity.Property(e => e.Ifnsul)
                    .HasColumnName("ifnsul")
                    .HasMaxLength(4)
                    .HasComment("Код ИФНС ЮЛ");

                entity.Property(e => e.Livestatus)
                    .HasColumnName("livestatus")
                    .HasComment("Признак действующего адресного объекта");

                entity.Property(e => e.Nextid)
                    .HasColumnName("nextid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор записи  связывания с последующей исторической записью");

                entity.Property(e => e.Normdoc)
                    .HasColumnName("normdoc")
                    .HasMaxLength(36)
                    .HasComment("Внешний ключ на нормативный документ");

                entity.Property(e => e.Number)
                    .HasColumnName("number")
                    .HasMaxLength(120)
                    .HasComment("Номер земельного участка");

                entity.Property(e => e.Okato)
                    .HasColumnName("okato")
                    .HasMaxLength(11)
                    .HasComment("OKATO");

                entity.Property(e => e.Oktmo)
                    .HasColumnName("oktmo")
                    .HasMaxLength(11)
                    .HasComment("OKTMO");

                entity.Property(e => e.Operstatus)
                    .HasColumnName("operstatus")
                    .HasComment("Статус действия над записью – причина появления записи (см. описание таблицы OperationStatus):");

                entity.Property(e => e.Parentguid)
                    .HasColumnName("parentguid")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор объекта родительского объекта");

                entity.Property(e => e.Postalcode)
                    .HasColumnName("postalcode")
                    .HasMaxLength(6)
                    .HasComment("Почтовый индекс");

                entity.Property(e => e.Previd)
                    .HasColumnName("previd")
                    .HasMaxLength(36)
                    .HasComment("Идентификатор записи связывания с предыдушей исторической записью");

                entity.Property(e => e.Regioncode)
                    .IsRequired()
                    .HasColumnName("regioncode")
                    .HasMaxLength(2)
                    .HasComment("Код региона");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date")
                    .HasComment("Начало действия записи");

                entity.Property(e => e.Steadguid)
                    .IsRequired()
                    .HasColumnName("steadguid")
                    .HasMaxLength(36)
                    .HasComment("Глобальный уникальный идентификатор адресного объекта (земельного участка)");

                entity.Property(e => e.Steadid)
                    .IsRequired()
                    .HasColumnName("steadid")
                    .HasMaxLength(36)
                    .HasComment("Уникальный идентификатор записи. Ключевое поле.");

                entity.Property(e => e.Terrifnsfl)
                    .HasColumnName("terrifnsfl")
                    .HasMaxLength(4)
                    .HasComment("Код территориального участка ИФНС ФЛ");

                entity.Property(e => e.Terrifnsul)
                    .HasColumnName("terrifnsul")
                    .HasMaxLength(4)
                    .HasComment("Код территориального участка ИФНС ЮЛ");

                entity.Property(e => e.Updatedate)
                    .HasColumnName("updatedate")
                    .HasColumnType("date")
                    .HasComment("Дата  внесения записи");
            });

            modelBuilder.Entity<Strstat>(entity =>
            {
                entity.ToTable("strstat", "fias");

                entity.HasComment("Состав и структура файла с информацией по признакам строения в БД ФИАС");

                entity.Property(e => e.Strstatid)
                    .HasColumnName("strstatid")
                    .HasComment("Признак строения")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20)
                    .HasComment("Наименование");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname")
                    .HasMaxLength(20)
                    .HasComment("Краткое наименование");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
