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
		('Parametros',     'CRM Dynamic Params',         'system-params',     6, 'Database'),
		('Horarios',     'Business opening hour',         'opening-hour',     7, 'CalendarClock'),
		('Turnos',     'Business work shift',         'work-shift',     8, 'CalendarCog'),
		('Cerrar Sesión',        'Redirects to login', 'login',        9, 'LogOut');
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
		--(1, @AdminRoleId),
		(2, @AdminRoleId),
		(3, @AdminRoleId),
		--(4, @AdminRoleId),
		(5, @AdminRoleId),
		(6, @AdminRoleId),
		(7, @AdminRoleId),
		(8, @AdminRoleId),
		(9, @AdminRoleId);
	END

	-- Support role menus
	IF NOT EXISTS (
	  SELECT 1 FROM auth.RoleMenus
	  WHERE RoleId = @SupportRoleId AND MenuId = 1
	)
	BEGIN
	  INSERT INTO auth.RoleMenus (MenuId, RoleId)
	  VALUES
		--(1, @SupportRoleId),
		(2, @SupportRoleId),
		--(4, @SupportRoleId),
		(5, @SupportRoleId),
		(9, @SupportRoleId);
	END

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
	THROW;
END CATCH;
GO

USE CustomerSupportDB;
GO
SELECT * FROM auth.SystemParams
GO
INSERT INTO auth.SystemParams
    ([Name], [Value], [Description], [Type], [CreatedAt], [CreateBy], [IsActive])
VALUES
    (
      'WelcomeBot',
      N'¡Soy *Milena*, tu asistente virtual de atención al cliente 🤖. Estoy aquí para brindarte información útil y optimizar tu tiempo. ¿En qué puedo ayudarte hoy?',
      N'Mensaje de saludo del bot',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'AskFullName',
      N'¡Hola 👋 Bienvenido a PC GROUP S.A.! Para comenzar, por favor indícanos tu *nombre completo* (al proporcionarlo, nos das tu permiso para registrar y usar tu información de manera segura).',
      N'Mensaje solicitud nombre completo',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'AskIdCard',
      N'Gracias, {0}. Ahora envíanos tu *número de cédula* (formato: 001-120203-1062W o 0011202031062W).',
      N'Mensaje solicitud número de cédula',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'InvalidIdFormat',
      N'😕 Formato inválido. Debe ser 13 caracteres (3 dígitos + 6 dígitos + 4 dígitos + letra), con o sin guiones (ej: 001-120203-1062W o 0011202031062W).',
      N'Mensaje Formato inválido',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'DataComplete',
      N'🎉 ¡Excelente! Has quedado registrado exitosamente y ¡bienvenido a nuestra agenda de clientes! A continuación continuamos con tu consulta.',
      N'Mensaje datos completos',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'InactivityWarning',
      N'⚠️ No hemos recibido respuesta en un tiempo. Tu conversación se cerrará pronto por inactividad. Si deseas continuar, envía cualquier mensaje.',
      N'Mensaje alerta tiempo inactivo',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'InactivityClosed',
      N'🔒 Tu conversación se cierra por inactividad. Seguimos aquí para cuando nos necesites. ¡Que tengas un buen día!',
      N'Mensaje alerta cierre conversación',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'SupportRequestReceived',
      N'✅ ¡Gracias! Hemos recibido tu solicitud de atención por un agente humano. Un miembro de nuestro equipo te atenderá en breve.',
      N'Mensaje confirmación atención agente de soporte',
      N'Prompts',
      GETDATE(),
      2,
      1
    ),
    (
      'Keywords',
      N'["agente", "humano", "operador"]',
      N'Validar intención de atención de agente',
      N'Keywords',
      GETDATE(),
      2,
      1
    ),
    (
      'InactivityWarningThresholdTime',
      N'2',
      N'Mensaje alerta de tiempo inactividad de la conversación',
      N'Temp',
      GETDATE(),
      2,
      1
    ),
    (
      'WaitWarningCloseTime',
      N'4',
      N'Mensaje alerta de cierre de conversación por inactividad',
      N'Temp',
      GETDATE(),
      2,
      1
    ),
	    (
     'InactivityWarningThresholdMesssage',
      N'⚠️ Ha transcurrido un periodo de inactividad. Para continuar la conversación, escriba un mensaje.',
      N'Mensaje de saludo del bot',
      N'Temp',
      GETDATE(),
      2,
      1
    ),
    (
      'WaitWarningCloseMesssage',
      N'⌛ El tiempo de espera ha concluido y la conversación se ha cerrado. Gracias por contactarnos.',
      N'Mensaje solicitud nombre completo',
      N'Temp',
      GETDATE(),
      2,
      1
    );
GO


USE crmpcg;

SELECT * FROM chat.Attachments;
SELECT * FROM chat.[Messages];
SELECT * FROM chat.ConversationHistoryLog;
SELECT * FROM chat.Conversations;
SELECT * FROM chat.NotificationRecipients;
SELECT * FROM chat.Notifications;
SELECT * FROM auth.ContactLogs;
SELECT * FROM chat.ConversationHistoryLog;
SELECT * FROM chat.Conversations;
SELECT * FROM crm.OpeningHour;
SELECT * FROM crm.WorkShift_User;
SELECT * FROM auth.SystemParams;

---- Delete test data
-- DELETE crm.OpeningHour;
-- DELETE crm.Workshift_User;
DELETE chat.Attachments;
DELETE chat.[Messages];
DELETE chat.ConversationHistoryLog;
DELETE chat.Conversations;
DELETE chat.NotificationRecipients;
DELETE chat.Notifications;
DELETE auth.ContactLogs;
DELETE auth.AuthTokens;
DELETE chat.ConversationHistoryLog;
DELETE chat.Conversations;
-- DELETE auth.SystemParams;


DELETE crm.OpeningHour WHERE Id = 10


GO
SET IDENTITY_INSERT [crm].[OpeningHour] ON 

INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (2, N'Horario L-V 08:00-17:30', N'Atención de lunes a viernes', CAST(N'08:00:00' AS Time), CAST(N'17:30:00' AS Time), 1, CAST(N'2025-06-24T05:45:51.4766667' AS DateTime2), NULL, 1, NULL, NULL, N'Monday,Tuesday,Wednesday,Thursday,Friday', NULL, NULL, N'Weekly', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (3, N'Horario Sáb 08:00-17:00', N'Atención el sábado', CAST(N'08:00:00' AS Time), CAST(N'17:00:00' AS Time), 1, CAST(N'2025-06-24T05:45:51.4800000' AS DateTime2), NULL, 1, NULL, NULL, N'Saturday', NULL, NULL, N'Weekly', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (4, N'Horario Dom 13:00-18:00', N'Atención el domingo', CAST(N'13:00:00' AS Time), CAST(N'18:00:00' AS Time), 1, CAST(N'2025-06-24T05:45:51.4800000' AS DateTime2), NULL, 1, NULL, NULL, N'Sunday', NULL, NULL, N'Weekly', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (5, N'Feriado 15/09', N'Día de la Independencia', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 1, CAST(N'2025-06-24T05:45:51.4800000' AS DateTime2), NULL, 1, NULL, N'15/09', NULL, NULL, NULL, N'AnnualHoliday', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (6, N'Feriado 19/04/2025', N'Decreto extraordinario', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 1, CAST(N'2025-06-24T05:45:51.4800000' AS DateTime2), NULL, 1, NULL, NULL, NULL, NULL, NULL, N'OneTimeHoliday', CAST(N'2025-04-19' AS Date))
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (7, N'Feriado móvil 01-07/06/2025', N'Periodo especial de receso', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 1, CAST(N'2025-06-24T05:45:51.4800000' AS DateTime2), NULL, 1, NULL, NULL, NULL, CAST(N'2025-06-01' AS Date), CAST(N'2025-06-07' AS Date), N'OneTimeHoliday', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (8, N'Dia', N'Horario de prueba de dia', CAST(N'05:00:00' AS Time), CAST(N'09:00:00' AS Time), 1, CAST(N'2025-06-24T07:05:21.0000000' AS DateTime2), CAST(N'2025-06-24T13:05:21.2400038' AS DateTime2), 2, NULL, NULL, N'Saturday', CAST(N'2025-06-24' AS Date), CAST(N'2025-06-24' AS Date), N'Weekly', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (9, N'Uno', N'Dos', NULL, NULL, 1, CAST(N'2025-06-24T07:45:06.0000000' AS DateTime2), CAST(N'2025-06-24T13:45:06.4674557' AS DateTime2), 2, NULL, N'24/06', NULL, NULL, NULL, N'AnnualHoliday', NULL)
INSERT [crm].[OpeningHour] ([Id], [Name], [Description], [StartTime], [EndTime], [IsActive], [CreatedAt], [UpdatedAt], [CreatedById], [UpdatedById], [HolidayDate], [DaysOfWeek], [EffectiveFrom], [EffectiveTo], [Recurrence], [SpecificDate]) VALUES (10, N'prueba 3', N'prueba 3', NULL, NULL, 1, CAST(N'2025-06-24T07:54:45.0000000' AS DateTime2), CAST(N'2025-06-24T13:54:45.8526437' AS DateTime2), 2, NULL, N'24/06', NULL, NULL, NULL, N'AnnualHoliday', NULL)
SET IDENTITY_INSERT [crm].[OpeningHour] OFF
GO


USE crmpcg;

SELECT * FROM chat.Attachments;
SELECT * FROM chat.[Messages];
SELECT * FROM chat.ConversationHistoryLog;
SELECT * FROM chat.Conversations;
SELECT * FROM chat.NotificationRecipients;
SELECT * FROM chat.Notifications;
SELECT * FROM auth.ContactLogs;
SELECT * FROM chat.ConversationHistoryLog;
SELECT * FROM chat.Conversations;
SELECT * FROM crm.OpeningHour;
SELECT * FROM crm.WorkShift_User;
SELECT * FROM auth.SystemParams;
SELECT * FROM auth.RoleMenus;
SELECT * FROM auth.Menus;

---- Delete test data
-- DELETE crm.OpeningHour;
-- DELETE crm.Workshift_User;
DELETE chat.Attachments;
DELETE chat.[Messages];
DELETE chat.ConversationHistoryLog;
DELETE chat.Conversations;
DELETE chat.NotificationRecipients;
DELETE chat.Notifications;
DELETE auth.ContactLogs;
DELETE auth.AuthTokens;
DELETE chat.ConversationHistoryLog;
DELETE chat.Conversations;
-- DELETE auth.SystemParams;


DELETE crm.OpeningHour WHERE Id BETWEEN 17 AND 18
DELETE crm.WorkShift_User

UPDATE auth.RoleMenus SET MenuId = 9 WHERE MenuId = 7 AND RoleId = 2


UPDATE crm.OpeningHour SET StartTime = NULL, EndTime = NULL WHERE Id BETWEEN 11 AND 16

UPDATE crm.OpeningHour SET Recurrence = 'OneTimeHoliday' WHERE Id = 5

UPDATE crm.OpeningHour SET IsWorkShift = 0 WHERE Id BETWEEN 2 AND 4