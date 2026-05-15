IF EXISTS (SELECT 1 FROM Users WHERE Username = 'demo123')
BEGIN
	UPDATE Users
	SET PasswordHash = 'demo123'
	WHERE Username = 'demo123';
END
ELSE
BEGIN
	INSERT INTO Users (UserId, Username, PasswordHash, CreatedAt)
	VALUES (NEWID(), 'demo123', 'demo123', GETDATE());
END
