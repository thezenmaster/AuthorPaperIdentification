ALTER TABLE paperauthor ADD COLUMN paperauthorid integer;
ALTER TABLE paperauthor ALTER COLUMN paperauthorid SET DEFAULT nextval('paperauthor_index_seq'::regclass);
ALTER TABLE paperauthor ALTER COLUMN paperauthorid SET NOT NULL;

ALTER TABLE paperauthor
  ADD CONSTRAINT paperauthor_pk PRIMARY KEY(paperauthorid);

ALTER TABLE trainconfirmed ADD COLUMN trainconfirmedid integer;
ALTER TABLE trainconfirmed ALTER COLUMN trainconfirmedid SET DEFAULT nextval('trainconfirmed_trainconfirmedid_seq'::regclass);
ALTER TABLE trainconfirmed ALTER COLUMN trainconfirmedid SET NOT NULL;

ALTER TABLE trainconfirmed
  ADD CONSTRAINT trainconfirmed_pk PRIMARY KEY(trainconfirmedid);

ALTER TABLE traindeleted ADD COLUMN traindeletedid integer;
ALTER TABLE traindeleted ALTER COLUMN traindeletedid SET DEFAULT nextval('traindeleted_traindeletedid_seq'::regclass);
ALTER TABLE traindeleted ALTER COLUMN traindeletedid SET NOT NULL;

ALTER TABLE traindeleted
  ADD CONSTRAINT traindeleted_pk PRIMARY KEY(traindeletedid);

ALTER TABLE validpaper ADD COLUMN validpaperid integer;
ALTER TABLE validpaper ALTER COLUMN validpaperid SET DEFAULT nextval('validpaper_validpaperid_seq'::regclass);
ALTER TABLE validpaper ALTER COLUMN validpaperid SET NOT NULL;

ALTER TABLE validpaper
  ADD CONSTRAINT validpaper_pk PRIMARY KEY(validpaperid);