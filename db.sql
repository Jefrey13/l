USE [CustomerSupportDB];
GO

-- =================================================================================
-- Seed data: Companies, Roles and Admin User
-- =================================================================================

-- 1) Insert 6 companies (including PC Group S.A) + dirección
INSERT INTO crm.Companies (Name, Address) VALUES
  ('PC Group S.A',             'Av. Central 123, Ciudad'),
  ('Distribuidora La Esquina', 'Calle Primera 45, Colonia'),
  ('Panadería El Trigal',      'Calle Pan 10, Barrio'),
  ('Farmacia Vida Sana',       'Av. Salud 78, Zona'),
  ('Hotel Paraíso Colonial',   'Carrera 5 #34-56, Centro'),
  ('Librería El Saber',        'Calle Libro 200, Distrito');
GO

-- 2) Insert AppRoles (if not exists)
INSERT INTO auth.AppRoles (RoleName, Description)
SELECT v.RoleName, v.Description
FROM (VALUES
   ('Admin',    'System administrator with full access'),
   ('Support',  'Customer support personnel in charge of communication')
) AS v(RoleName, Description)
WHERE NOT EXISTS (
  SELECT 1 FROM auth.AppRoles r
   WHERE r.RoleName = v.RoleName
);
GO

-- 3+4) Insert admin user for PC Group S.A (si no existe) + asignar roles
DECLARE
  @AdminUserId   INT,
  @PCGroupId     INT,
  @AdminRoleId   INT,
  @SupportRoleId INT;

/* Obtener CompanyId */
SELECT @PCGroupId = CompanyId
  FROM crm.Companies
 WHERE Name = 'PC Group S.A';

/* Insertar usuario administrador si no existe */
IF NOT EXISTS (
  SELECT 1 FROM auth.Users u
   WHERE u.Email = 'diviv78453@jazipo.com'
)
BEGIN
  INSERT INTO auth.Users (
      FullName,
      Email,
      PasswordHash,
      IsActive,
      CompanyId,
      DataRequested,    -- obligatorio
      ImageUrl
  )
  VALUES (
    'Jefrey Antonio Zuniga Rivera',
    'diviv78453@jazipo.com',
    CONVERT(VARBINARY(256), '$2a$12$I61KQba9vQ58Usx6nFjxP.mOF6RXikH.mmi5lgfkOZRFiCCC0sJma'),
    1,
    @PCGroupId,
    0,  -- DataRequested = false
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

/* Obtener RoleId de Admin y Support */
SELECT @AdminRoleId   = RoleId FROM auth.AppRoles WHERE RoleName = 'Admin';

/* Asignar rol Admin */
IF NOT EXISTS (
  SELECT 1 FROM auth.UserRoles ur
   WHERE ur.UserId = @AdminUserId
     AND ur.RoleId = @AdminRoleId
)
  INSERT INTO auth.UserRoles (UserId, RoleId)
  VALUES (@AdminUserId, @AdminRoleId);
GO

-- =================================================================================
-- Seed data: Companies, Roles and Admin User
-- =================================================================================

-- 3+4) Insert admin user for PC Group S.A (si no existe) + asignar roles
DECLARE
  @AdminUserId   INT,
  @PCGroupId     INT,
  @AdminRoleId   INT,
  @SupportRoleId INT;

SELECT @PCGroupId = CompanyId
  FROM crm.Companies
 WHERE Name = 'PC Group S.A';

/* Insertar usuario support si no existe */
IF NOT EXISTS (
  SELECT 1 FROM auth.Users u
   WHERE u.Email = 'ladoral441@neuraxo.com'
)
BEGIN
  INSERT INTO auth.Users (
      FullName,
      Email,
      PasswordHash,
      IsActive,
      CompanyId,
      DataRequested,    -- obligatorio
      ImageUrl
  )
  VALUES (
    'Suporte',
    'ladoral441@neuraxo.com',
    CONVERT(VARBINARY(256), '$2a$12$I61KQba9vQ58Usx6nFjxP.mOF6RXikH.mmi5lgfkOZRFiCCC0sJma'),
    1,
    @PCGroupId,
    0,  -- DataRequested = false
    'https://i.ibb.co/CpHzbxg9/logopcg.webp'
  );

  SET @AdminUserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
  SELECT @AdminUserId = UserId
    FROM auth.Users
   WHERE Email = '$2a$12$I61KQba9vQ58Usx6nFjxP.mOF6RXikH.mmi5lgfkOZRFiCCC0sJma';
END


SELECT @SupportRoleId = RoleId FROM auth.AppRoles WHERE RoleName = 'Support';

/* Asignar rol Support */
IF NOT EXISTS (
  SELECT 1 FROM auth.UserRoles ur
   WHERE ur.UserId = @AdminUserId
     AND ur.RoleId = @SupportRoleId
)
  INSERT INTO auth.UserRoles (UserId, RoleId)
  VALUES (@AdminUserId, @SupportRoleId);
GO


GO
BEGIN
	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Dashboard', 'Analisi de los KPI de las interacciónes con los clientes', 'Dashboard', 1, 'LayoutDashboard');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Chat', 'Pagina de mensajes entre PC GROUP S.A. y clientes', 'Chat', 2, 'MessageSquare');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Usuarios', 'Listados de usuarios en el sistema', 'Users', 3, 'Users');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Notificaciones', 'Asignación de conversación, nuevo usuario, etc.', 'Notifications', 4, 'BellRing');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Perfil', 'Perfil a detalle del usuario', 'Profile', 5, 'User');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Cerrar Sesión', 'Redirije por defecto al inicio de sesión', 'LogOut', 6, 'LogOut');
END

SELECT *  FROM auth.Menus;

GO
BEGIN
	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 1),
		(2, 1),
		(3, 1),
		(4, 1),
		(5, 1),
		(6, 1)


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 2),
		(2, 2),
		(4, 2),
		(5, 2),
		(6, 2);
END

USE [CustomerSupportDB];
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- =================================================================================
    -- 1) Insert Companies
    -- =================================================================================
    INSERT INTO crm.Companies (Name, Address)
    VALUES
      ('PC Group S.A',             'Av. Central 123, Ciudad'),
      ('Distribuidora La Esquina', 'Calle Primera 45, Colonia'),
      ('Panadería El Trigal',      'Calle Pan 10, Barrio'),
      ('Farmacia Vida Sana',       'Av. Salud 78, Zona'),
      ('Hotel Paraíso Colonial',   'Carrera 5 #34-56, Centro'),
      ('Librería El Saber',        'Calle Libro 200, Distrito')
    ;

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
    )
    ;

    -- =================================================================================
    -- 3) Seed admin and support users and assign roles
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
        'Jefrey Antonio Zuniga Rivera',
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
        'Support Agent',
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
    IF NOT EXISTS (SELECT 1 FROM auth.Menus WHERE Url = 'Dashboard')
    BEGIN
      INSERT INTO auth.Menus ([Name], [Description], [Url], [Index], [Icon])
      VALUES
        ('Dashboard',     'KPI overview of customer interactions',         'Dashboard',     1, 'LayoutDashboard'),
        ('Chat',          'Messaging between PC Group S.A. and clients', 'Chat',          2, 'MessageSquare'),
        ('Users',         'List of system users',                         'Users',         3, 'Users'),
        ('Notifications', 'Conversation assignment, new users, etc.',     'Notifications', 4, 'BellRing'),
        ('Profile',       'User profile details',                         'Profile',       5, 'User'),
        ('LogOut',        'Redirects to login',                           'LogOut',        6, 'LogOut');
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



---------------------
BEGIN TRANSACTION;

DECLARE @ContactId    INT,
        @ConversationId INT;

-- 1) Insert a new contact
INSERT INTO auth.ContactLogs
    (FullName, Phone, WaId, WaName)
VALUES
    ('Test Client', '+10000000000', 'waid_test', 'TestClient');

SET @ContactId = SCOPE_IDENTITY();

-- 2) Insert a new conversation for that contact, assigned to user 1
INSERT INTO chat.Conversations
    (CompanyId, ClientContactId, AssignedAgentId, AssignedByUserId, Status, Initialized, CreatedAt, UpdatedAt)
VALUES
    (1, @ContactId, 1, 1, 'New', 0, SYSUTCDATETIME(), SYSUTCDATETIME());

SET @ConversationId = SCOPE_IDENTITY();

-- 3) Insert a new message from the client in that conversation
INSERT INTO chat.Messages
    (ConversationId, SenderContactId, Content, ExternalId, MessageType, DeliveredAt)
VALUES
    (@ConversationId, @ContactId, 'Hello, I need assistance', NEWID(), 'text', SYSUTCDATETIME());

COMMIT;



USE CustomerSupportDB;

SELECT * FROM auth.AppRoles;
SELECT * FROM auth.Users;
SELECT * FROM auth.UserRoles;
SELECT * FROM auth.AuthTokens;;
SELECT * FROM chat.Conversations
SELECT * FROM chat.Messages
SELECT * FROM auth.Menus
SELECT * FROM auth.RoleMenus
SELECT * FROM auth.ContactLogs


--DELETE auth.UserRoles;
--DELETE auth.Users WHERE UserId = 4;
--DELETE auth.RoleMenus;
DELETE auth.UserRoles;
DELETE auth.ContactLogs WHERE Id =15;
-- DELETE auth.RoleMenus;
