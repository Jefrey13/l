USE [crmpcg]
GO
/****** Object:  Table [crm].[OpeningHour]    Script Date: 24/6/2025 08:28:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crm].[OpeningHour](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime2](7) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedById] [int] NULL,
	[UpdatedById] [int] NULL,
	[HolidayDate] [nvarchar](5) NULL,
	[DaysOfWeek] [nvarchar](50) NULL,
	[EffectiveFrom] [date] NULL,
	[EffectiveTo] [date] NULL,
	[Recurrence] [nvarchar](20) NOT NULL,
	[SpecificDate] [date] NULL,
 CONSTRAINT [PK_OpeningHour] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [crm].[WorkShift_User]    Script Date: 24/6/2025 08:28:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crm].[WorkShift_User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OpeningHourId] [int] NOT NULL,
	[AssignedUserId] [int] NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedById] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
 CONSTRAINT [PK_WorkShift_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
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
SET IDENTITY_INSERT [crm].[WorkShift_User] ON 

INSERT [crm].[WorkShift_User] ([Id], [OpeningHourId], [AssignedUserId], [CreatedById], [UpdatedById], [IsActive], [CreatedAt], [UpdatedAt], [ValidFrom], [ValidTo]) VALUES (9, 3, 3, 2, NULL, 1, CAST(N'2025-06-24T08:09:00.0000000' AS DateTime2), CAST(N'2025-06-24T14:09:00.8151081' AS DateTime2), CAST(N'2025-06-24' AS Date), CAST(N'2025-06-24' AS Date))
SET IDENTITY_INSERT [crm].[WorkShift_User] OFF
GO
ALTER TABLE [crm].[OpeningHour] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [crm].[OpeningHour] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [crm].[OpeningHour] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [crm].[OpeningHour] ADD  DEFAULT (N'') FOR [Recurrence]
GO
ALTER TABLE [crm].[WorkShift_User] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [crm].[WorkShift_User] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [crm].[WorkShift_User] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [crm].[OpeningHour]  WITH CHECK ADD  CONSTRAINT [FK_OpeningHour_CreatedBy] FOREIGN KEY([CreatedById])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [crm].[OpeningHour] CHECK CONSTRAINT [FK_OpeningHour_CreatedBy]
GO
ALTER TABLE [crm].[OpeningHour]  WITH CHECK ADD  CONSTRAINT [FK_OpeningHour_UpdatedBy] FOREIGN KEY([UpdatedById])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [crm].[OpeningHour] CHECK CONSTRAINT [FK_OpeningHour_UpdatedBy]
GO
ALTER TABLE [crm].[WorkShift_User]  WITH CHECK ADD  CONSTRAINT [FK_OpeningHour_WorkShift_User] FOREIGN KEY([OpeningHourId])
REFERENCES [crm].[OpeningHour] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [crm].[WorkShift_User] CHECK CONSTRAINT [FK_OpeningHour_WorkShift_User]
GO
ALTER TABLE [crm].[WorkShift_User]  WITH CHECK ADD  CONSTRAINT [FK_WorkShift_User_AssignedUser] FOREIGN KEY([AssignedUserId])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [crm].[WorkShift_User] CHECK CONSTRAINT [FK_WorkShift_User_AssignedUser]
GO
ALTER TABLE [crm].[WorkShift_User]  WITH CHECK ADD  CONSTRAINT [FK_WorkShift_User_CreatedBy] FOREIGN KEY([CreatedById])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [crm].[WorkShift_User] CHECK CONSTRAINT [FK_WorkShift_User_CreatedBy]
GO
ALTER TABLE [crm].[WorkShift_User]  WITH CHECK ADD  CONSTRAINT [FK_WorkShift_User_UpdatedBy] FOREIGN KEY([UpdatedById])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [crm].[WorkShift_User] CHECK CONSTRAINT [FK_WorkShift_User_UpdatedBy]
GO
