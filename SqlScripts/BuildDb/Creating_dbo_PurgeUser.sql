CREATE PROCEDURE [dbo].[PurgeUser](@uid INT)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DELETE dbo.UserRole
	FROM dbo.UserRole ur
	JOIN dbo.Users u ON u.UserId = ur.UserId
	WHERE u.UserId = @uid
	
	DELETE dbo.ActivityLog
	FROM dbo.ActivityLog a
	JOIN dbo.Users u ON a.UserId = u.UserId
	WHERE u.UserId = @uid
	
	delete dbo.Preferences
	from dbo.Preferences p
	join dbo.Users u on u.UserId = p.UserId
	where u.UserId = @uid
	
	DELETE dbo.VolunteerForm
	FROM dbo.VolunteerForm vf
	JOIN dbo.Users u ON vf.UploaderId = u.UserId
	WHERE u.UserId = @uid
	
	DELETE dbo.Coupons
	FROM dbo.Coupons c
	JOIN dbo.Users u ON c.UserId = u.UserId
	WHERE u.UserId = @uid
	
	DELETE dbo.Users
	WHERE UserId = @uid
	
END
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO