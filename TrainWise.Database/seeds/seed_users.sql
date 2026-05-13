IF EXISTS (SELECT 1 FROM Users WHERE Username = 'demo')
BEGIN
	UPDATE Users
	SET PasswordHash = 'demo'
	WHERE Username = 'demo';
END
ELSE
BEGIN
	INSERT INTO Users (UserId, Username, PasswordHash, CreatedAt)
	VALUES (NEWID(), 'demo', 'demo', GETDATE());
END
