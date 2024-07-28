**Indices to import data from xml files:**

```
CREATE UNIQUE INDEX idx_actstatid ON fias.actstat(actstatid);
CREATE UNIQUE INDEX idx_addrobjid ON fias.addrobj(aoid);
CREATE UNIQUE INDEX idx_centerstid ON fias.centerst(centerstid);
CREATE UNIQUE INDEX idx_curentstid ON fias.curentst(curentstid);
CREATE UNIQUE INDEX idx_eststatid ON fias.eststat(eststatid);
CREATE UNIQUE INDEX idx_fltypeid ON fias.flattype(fltypeid);
CREATE UNIQUE INDEX idx_houseid ON fias.house(houseid);
CREATE UNIQUE INDEX idx_ndtypeid ON fias.ndoctype(ndtypeid);
CREATE UNIQUE INDEX idx_normdocid ON fias.normdoc(normdocid);
CREATE UNIQUE INDEX idx_operstatid ON fias.operstat(operstatid);
CREATE UNIQUE INDEX idx_roomid ON fias.room(roomid);
CREATE UNIQUE INDEX idx_rmtypeid ON fias.roomtype(rmtypeid);
CREATE UNIQUE INDEX idx_socrbaseid ON fias.socrbase(kod_t_st);
CREATE UNIQUE INDEX idx_steadid ON fias.stead(steadid);
CREATE UNIQUE INDEX idx_strstatid ON fias.strstat(strstatid);
```


1. to fix errors like **"relation 'actstat' does not exist"** we have to set *search_path* in psql.

``show search_path;`` - look at the current search schemas
``\dn`` in psql or ``select nspname from pg_catalog.pg_namespace;`` by clear sql look at the available schemas
``set search_path to public,fias;`` - add scheme fias to search path

2. ``create table fias.settings (key varchar(128), value varchar(4096));``

CREATE EXTENSION pg_trgm;
create index idx_addrobj_formalname on addrobj using gin (formalname gin_trgm_ops);
