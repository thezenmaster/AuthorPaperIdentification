ALTER TABLE validpaper
  ADD CONSTRAINT validpaper_paper FOREIGN KEY (paperid)
      REFERENCES paper (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION;