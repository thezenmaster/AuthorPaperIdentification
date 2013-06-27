ALTER TABLE paperkeyword ADD COLUMN count bigint;

-- column will contain the probability that a keyword is associated w/ a paper (w/ laplace smoothing)
ALTER TABLE paperkeyword ADD COLUMN normalizedcount double precision;

-- column will contain the popularity of the keyword (w/ laplace smoothing)
ALTER TABLE keyword ADD COLUMN normalizedcount double precision;