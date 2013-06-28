CREATE SEQUENCE paperauthor_index_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 25551642
  CACHE 1;
ALTER TABLE paperauthor_index_seq
  OWNER TO postgres;

    CREATE SEQUENCE trainconfirmed_trainconfirmedid_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 224924
  CACHE 1;
ALTER TABLE trainconfirmed_trainconfirmedid_seq
  OWNER TO postgres;

ALTER TABLE trainconfirmed_trainconfirmedid_seq
  OWNER TO postgres;
  
  CREATE SEQUENCE traindeleted_traindeletedid_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 224924
  CACHE 1;
ALTER TABLE traindeleted_traindeletedid_seq
  OWNER TO postgres;
  
CREATE SEQUENCE validpaper_validpaperid_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 180176
  CACHE 1;
ALTER TABLE validpaper_validpaperid_seq
  OWNER TO postgres;

ALTER TABLE paperauthor ADD COLUMN paperauthorid integer;
ALTER TABLE paperauthor ALTER COLUMN paperauthorid SET DEFAULT nextval('paperauthor_index_seq'::regclass);
update paperauthor set paperauthorid = nextval('paperauthor_index_seq'::regclass);
ALTER TABLE paperauthor ALTER COLUMN paperauthorid SET NOT NULL;

ALTER TABLE paperauthor
  ADD CONSTRAINT paperauthor_pk PRIMARY KEY(paperauthorid);

ALTER TABLE trainconfirmed ADD COLUMN trainconfirmedid integer;
ALTER TABLE trainconfirmed ALTER COLUMN trainconfirmedid SET DEFAULT nextval('trainconfirmed_trainconfirmedid_seq'::regclass);
update trainconfirmed set trainconfirmedid = nextval('trainconfirmed_trainconfirmedid_seq'::regclass);
ALTER TABLE trainconfirmed ALTER COLUMN trainconfirmedid SET NOT NULL;

ALTER TABLE trainconfirmed
  ADD CONSTRAINT trainconfirmed_pk PRIMARY KEY(trainconfirmedid);

ALTER TABLE traindeleted ADD COLUMN traindeletedid integer;
ALTER TABLE traindeleted ALTER COLUMN traindeletedid SET DEFAULT nextval('traindeleted_traindeletedid_seq'::regclass);
update traindeleted set traindeletedid = nextval('traindeleted_traindeletedid_seq'::regclass);
ALTER TABLE traindeleted ALTER COLUMN traindeletedid SET NOT NULL;

ALTER TABLE traindeleted
  ADD CONSTRAINT traindeleted_pk PRIMARY KEY(traindeletedid);

ALTER TABLE validpaper ADD COLUMN validpaperid integer;
ALTER TABLE validpaper ALTER COLUMN validpaperid SET DEFAULT nextval('validpaper_validpaperid_seq'::regclass);
update validpaper set validpaperid = nextval('validpaper_validpaperid_seq'::regclass);
ALTER TABLE validpaper ALTER COLUMN validpaperid SET NOT NULL;

ALTER TABLE validpaper
  ADD CONSTRAINT validpaper_pk PRIMARY KEY(validpaperid);