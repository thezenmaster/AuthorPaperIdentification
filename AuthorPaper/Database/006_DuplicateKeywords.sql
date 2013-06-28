-- Create a function that always returns the first non-NULL item
CREATE OR REPLACE FUNCTION public.first_agg ( anyelement, anyelement )
RETURNS anyelement LANGUAGE sql IMMUTABLE STRICT AS $$
        SELECT $1;
$$;
 
-- And then wrap an aggregate around it
CREATE AGGREGATE public.first (
        sfunc    = public.first_agg,
        basetype = anyelement,
        stype    = anyelement
);

CREATE OR REPLACE VIEW duplicate_keywords AS 
 SELECT keyword.value, count(keyword.value) AS count, 
    first(keyword.keywordid) AS keywordid,
        sum(keyword.count) as sumCount
   FROM keyword
  GROUP BY keyword.value
 HAVING count(keyword.value) > 1;

ALTER TABLE duplicate_keywords
  OWNER TO postgres;

  


CREATE INDEX keyword_value_ix
  ON keyword
  USING btree
  (value COLLATE pg_catalog."default" varchar_ops);


	CREATE OR REPLACE FUNCTION removeDuplicates() RETURNS int LANGUAGE plpgsql AS $$
	declare 
	duplicate record;
	updatePk record;
	BEGIN
	FOR duplicate IN (SELECT keywordid, value, sumCount from duplicate_keywords)
		LOOP

		UPDATE keyword SET count = duplicate.sumCount
		where keywordid = duplicate.keywordid;

		FOR updatePk IN (SELECT keywordid from keyword pk where pk.value = duplicate.value and pk.keywordid <> duplicate.keywordid)
		LOOP
			update paperkeyword
				set keywordid = duplicate.keywordid
				where keywordid = updatePk.keywordid;
					
			delete from keyword
			where keywordid = updatePk.keywordid;	  
		END LOOP;  
	END LOOP;

	RETURN 1;
	END
	$$;
	SELECT removeDuplicates();