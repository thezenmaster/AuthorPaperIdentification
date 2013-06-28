CREATE OR REPLACE FUNCTION totalDictCount() RETURNS int LANGUAGE plpgsql AS $$
DECLARE
  totalDictCount double precision;
BEGIN
  select sum(count) into totalDictCount
   from keyword;

   update keyword
set normalizedcount = "count" / totalDictCount;
   
   update paperkeyword
set normalizedcount = count / 
(select sum(count) from keyword k where k.keywordid = keywordid);
   
  RETURN totalDictCount;
END
$$;
SELECT totalDictCount();
