USE [CustomerSupportDB];
GO

BEGIN TRY
	BEGIN TRANSACTION;

	-- =================================================================================
	-- 1) Insert Companies
	-- =================================================================================
	INSERT INTO crm.Companies (Name, Description ,Address)
	VALUES
	  ('PC Group S.A', 'Desarrollo de software' ,            'Av. Central 123, Ciudad'),
	  ('Distribuidora La Esquina', 'Productos primarios' , 'Calle Primera 45, Colonia');

	-- =================================================================================
	-- 2) Insert AppRoles if not exists
	-- =================================================================================
	INSERT INTO auth.AppRoles (RoleName, Description)
	SELECT v.RoleName, v.Description
	FROM (VALUES
	   ('Admin',    'System administrator with full access'),
	   ('Support',  'Customer support personnel in charge of communication')
	) AS v(RoleName, Description)
	WHERE NOT EXISTS (
	  SELECT 1
	  FROM auth.AppRoles r
	  WHERE r.RoleName = v.RoleName
	);

	-- =================================================================================
	-- 3) Seed Bot, admin and support users and assign roles
	-- =================================================================================
	DECLARE
		@AdminUserId   INT,
		@SupportUserId INT,
		@PCGroupId     INT,
		@AdminRoleId   INT,
		@SupportRoleId INT;

	-- get PC Group company id
	SELECT @PCGroupId = CompanyId
	  FROM crm.Companies
	 WHERE Name = 'PC Group S.A';

	-- get role ids
	SELECT @AdminRoleId   = RoleId FROM auth.AppRoles WHERE RoleName = 'Admin';
	SELECT @SupportRoleId = RoleId FROM auth.AppRoles WHERE RoleName = 'Support';

	-- seed Support user
	IF NOT EXISTS (
	  SELECT 1 FROM auth.Users u WHERE u.Email = 'jefreyzunia13@gmail.com'
	)
	BEGIN
	  INSERT INTO auth.Users (
		  FullName,
		  Email,
		  PasswordHash,
		  IsActive,
		  CompanyId,
		  DataRequested,
		  ImageUrl
	  )
	  VALUES (
		'Bot',
		'ljefreyzunia13@gmail.com',
		CONVERT(VARBINARY(256), '$2a$12$I61KQba9vQ58Usx6nFjxP.mOF6RXikH.mmi5lgfkOZRFiCCC0sJma'),
		1,
		@PCGroupId,
		0,
		'https://i.ibb.co/G3xsSdpX/bot.webp'
	  );
	  SET @SupportUserId = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
	  SELECT @SupportUserId = UserId
		FROM auth.Users
	   WHERE Email = 'ladoral441@neuraxo.com';
	END

	-- assign Support role
	IF NOT EXISTS (
	  SELECT 1
	  FROM auth.UserRoles ur
	  WHERE ur.UserId = @SupportUserId
		AND ur.RoleId = @SupportRoleId
	)
	BEGIN
	  INSERT INTO auth.UserRoles (UserId, RoleId)
	  VALUES (@SupportUserId, @AdminRoleId);
	END


	-- seed Admin user
	IF NOT EXISTS (
	  SELECT 1 FROM auth.Users u WHERE u.Email = 'diviv78453@jazipo.com'
	)
	BEGIN
	  INSERT INTO auth.Users (
		  FullName,
		  Email,
		  PasswordHash,
		  IsActive,
		  CompanyId,
		  DataRequested,
		  ImageUrl
	  )
	  VALUES (
		'Alisa Mercedez López Cárdenas',
		'diviv78453@jazipo.com',
		CONVERT(VARBINARY(256), '$2a$12$I61KQba9vQ58Usx6nFjxP.mOF6RXikH.mmi5lgfkOZRFiCCC0sJma'),
		1,
		@PCGroupId,
		0,
		'https://i.ibb.co/CpHzbxg9/logopcg.webp'
	  );
	  SET @AdminUserId = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
	  SELECT @AdminUserId = UserId
		FROM auth.Users
	   WHERE Email = 'diviv78453@jazipo.com';
	END

	-- assign Admin role
	IF NOT EXISTS (
	  SELECT 1
	  FROM auth.UserRoles ur
	  WHERE ur.UserId = @AdminUserId
		AND ur.RoleId = @AdminRoleId
	)
	BEGIN
	  INSERT INTO auth.UserRoles (UserId, RoleId)
	  VALUES (@AdminUserId, @AdminRoleId);
	END

	-- seed Support user
	IF NOT EXISTS (
	  SELECT 1 FROM auth.Users u WHERE u.Email = 'ladoral441@neuraxo.com'
	)
	BEGIN
	  INSERT INTO auth.Users (
		  FullName,
		  Email,
		  PasswordHash,
		  IsActive,
		  CompanyId,
		  DataRequested,
		  ImageUrl
	  )
	  VALUES (
		'Sebastian de Los Angeles Gutiérrez',
		'ladoral441@neuraxo.com',
		CONVERT(VARBINARY(256), '$2a$12$I61KQba9vQ58Usx6nFjxP.mOF6RXikH.mmi5lgfkOZRFiCCC0sJma'),
		1,
		@PCGroupId,
		0,
		'https://i.ibb.co/CpHzbxg9/logopcg.webp'
	  );
	  SET @SupportUserId = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
	  SELECT @SupportUserId = UserId
		FROM auth.Users
	   WHERE Email = 'ladoral441@neuraxo.com';
	END

	-- assign Support role
	IF NOT EXISTS (
	  SELECT 1
	  FROM auth.UserRoles ur
	  WHERE ur.UserId = @SupportUserId
		AND ur.RoleId = @SupportRoleId
	)
	BEGIN
	  INSERT INTO auth.UserRoles (UserId, RoleId)
	  VALUES (@SupportUserId, @SupportRoleId);
	END

	-- =================================================================================
	-- 4) Seed Menus
	-- =================================================================================
	IF NOT EXISTS (SELECT 1 FROM auth.Menus WHERE Url = 'dashboard')
	BEGIN
	  INSERT INTO auth.Menus ([Name], [Description], [Url], [Index], [Icon])
	  VALUES
		('Dashboard',     'KPI overview of customer interactions',         'dashboard',     1, 'LayoutDashboard'),
		('Chat',          'Messaging between PC Group S.A. and clients', 'chat',          2, 'MessageSquare'),
		('Usuarios',         'List of system users',                         'users',         3, 'Users'),
		('Notificaciones ', 'Conversation assignment, new users, etc.',     'notifications', 4, 'BellRing'),
		('Perfil',       'User profile details',                         'profile',       5, 'User'),
		('Cerrar Sesión',        'Redirects to login',                           'login',        6, 'LogOut');
	END

	-- =================================================================================
	-- 5) Seed RoleMenus for Admin and Support
	-- =================================================================================
	-- Admin role menus
	IF NOT EXISTS (
	  SELECT 1 FROM auth.RoleMenus
	  WHERE RoleId = @AdminRoleId AND MenuId = 1
	)
	BEGIN
	  INSERT INTO auth.RoleMenus (MenuId, RoleId)
	  VALUES
		(1, @AdminRoleId),
		(2, @AdminRoleId),
		(3, @AdminRoleId),
		(4, @AdminRoleId),
		(5, @AdminRoleId),
		(6, @AdminRoleId);
	END

	-- Support role menus
	IF NOT EXISTS (
	  SELECT 1 FROM auth.RoleMenus
	  WHERE RoleId = @SupportRoleId AND MenuId = 1
	)
	BEGIN
	  INSERT INTO auth.RoleMenus (MenuId, RoleId)
	  VALUES
		(1, @SupportRoleId),
		(2, @SupportRoleId),
		(4, @SupportRoleId),
		(5, @SupportRoleId),
		(6, @SupportRoleId);
	END

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
	THROW;
END CATCH;
GO



-- ==================EXTRA ==========================
USE CustomerSupportDB;

SELECT * FROM chat.Conversations
SELECT * FROM chat.[Messages]
SELECT * FROM auth.ContactLogs;
SELECT * FROM auth.Users;
SELECT * FROM auth.AppRoles;
SELECT * FROM auth.UserRoles;
SELECT * FROM auth.Menus;
-----------------
DELETE chat.Conversations;
DELETE chat.[Messages];
DELETE auth.ContactLogs;

UPDATE auth.Menus SET [Url] = 'login' WHERE MenuId = 6
SELECT * FROM  crm.Companies;

UPDATE chat.Conversations SET Status = 'Closed'

UPDATE chat.Conversations SET Status  = 'Closed'  where ConversationId = 12