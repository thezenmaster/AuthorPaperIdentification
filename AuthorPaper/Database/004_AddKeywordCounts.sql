ALTER TABLE paperkeyword ADD COLUMN count bigint;

-- column will contain the probability that a keyword is associated w/ a paper
ALTER TABLE paperkeyword ADD COLUMN normalizedcount double precision;

-- column will contain the popularity of the keyword
ALTER TABLE keyword ADD COLUMN normalizedcount double precision;