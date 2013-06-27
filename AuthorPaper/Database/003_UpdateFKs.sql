UPDATE paper
   SET conferenceid=NULL
 WHERE (select count(*) from conference cf
 where cf.id = conferenceid) = 0;
 
 
UPDATE paper
   SET journalid=NULL
 WHERE (select count(*) from journal j
 where j.id = journalid) = 0;
 
 ALTER TABLE paper
  ADD CONSTRAINT paper_conference FOREIGN KEY (conferenceid)
      REFERENCES conference (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE paper
  ADD CONSTRAINT paper_journal FOREIGN KEY (journalid)
      REFERENCES journal (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION;
