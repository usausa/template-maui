INSERT INTO Work (Id, Name) VALUES ((SELECT COALESCE(MAX(Id), 0) + 1 FROM Work), /*@ name */'x')
