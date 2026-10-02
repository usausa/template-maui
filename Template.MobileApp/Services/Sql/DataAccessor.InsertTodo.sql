INSERT INTO
    Todo (Title, Note, DueDate, IsImportant, IsDone, CreatedAt, UpdatedAt)
VALUES
    (/*@ title */'', /*@ note */'', /*@ dueDate */'', /*@ isImportant */0, /*@ isDone */0, /*@ createdAt */'', /*@ updatedAt */'')
RETURNING
    Id
