ALTER TABLE validpaper
  ADD CONSTRAINT validpaper_paper FOREIGN KEY (paperid)
      REFERENCES paper (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION;

	  
ALTER TABLE paperkeyword ADD COLUMN maxcount bigint;

/*
CREATE OR REPLACE FUNCTION setMaxCount() RETURNS int LANGUAGE plpgsql AS $$
	declare 
	duplicate record;	
	BEGIN
	UPDATE paperkeyword SET maxcount = 1;
	
	FOR duplicate IN (select paperid, max(count) as maxcount from paperkeyword group by paperid having max(count) > 1)
	LOOP
		UPDATE paperkeyword SET maxcount = duplicate.maxcount
		where paperid = duplicate.paperid;
	END LOOP;

	RETURN 1;
	END
	$$;
SELECT setMaxCount();

*/

CREATE OR REPLACE VIEW trainpaper AS 
 SELECT DISTINCT validpaper.paperid
   FROM validpaper;

ALTER TABLE trainpaper
  OWNER TO postgres;


delete from validpaper
where validpaperid in
(
select first(validpaperid)
from validpaper
group by paperid
having count(paperid)>1
)
