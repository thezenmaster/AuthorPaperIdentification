CREATE TABLE keyword
(
  count bigint,
  value character varying(256),
  normalizedcount double precision,
  keywordid serial NOT NULL,
  CONSTRAINT keyword_pk PRIMARY KEY (keywordid)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE keyword
  OWNER TO postgres;

  
CREATE TABLE paperkeyword
(
  paperid bigint NOT NULL,
  count bigint,
  normalizedcount double precision,
  keywordid integer NOT NULL,
  paperkeywordid serial NOT NULL,
  CONSTRAINT paperkeyword_pk PRIMARY KEY (paperkeywordid),
  CONSTRAINT paperkeyword_keyword FOREIGN KEY (keywordid)
      REFERENCES keyword (keywordid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE CASCADE,
  CONSTRAINT paperkeyword_paper FOREIGN KEY (paperid)
      REFERENCES paper (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
ALTER TABLE paperkeyword
  OWNER TO postgres;

-- Index: fki_paperkeyword_keyword

-- DROP INDEX fki_paperkeyword_keyword;

CREATE INDEX fki_paperkeyword_keyword
  ON paperkeyword
  USING btree
  (keywordid);




