IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL
	DROP TABLE [dbo].[Accounts];

CREATE TABLE [dbo].[Accounts]
(
	[AccountId]			[uniqueidentifier]						NOT NULL,
	[AccountName]						[nvarchar](100)				NOT NULL,
	[Description]						[nvarchar](100)				NOT NULL,
	[IsActive]			[bit]				NOT NULL	CONSTRAINT [DF_Accounts_IsActive]	DEFAULT(0),
	[IsMarkedForDeletion]			[bit]				NOT NULL	CONSTRAINT [DF_Accounts_IsMarkedForDeletion]	DEFAULT(0),
	[CreatedFromRemoteIpAddress]					[varchar](45)				NOT NULL	CONSTRAINT [DF_Accounts_CreatedFromRemoteIpAddress]	DEFAULT(''),
	[CreatedBy]						[nvarchar](100)				NOT NULL	CONSTRAINT [DF_Accounts_CreatedBy]	DEFAULT(''),
	[CreatedUtcDate]				[datetime]					NOT NULL	CONSTRAINT [DF_Accounts_CreatedUtcDate]	DEFAULT(GETUTCDATE()),
	[CreatedUserAgent]						[nvarchar](200)				NOT NULL	CONSTRAINT [DF_Accounts_CreatedUserAgent]	DEFAULT(''),
	[UpdatedFromRemoteIpAddress]		[varchar](45)				NOT NULL	CONSTRAINT [DF_Accounts_UpdatedFromRemoteIpAddress]	DEFAULT(''),
	[UpdatedBy]						[nvarchar](100)				NOT NULL	CONSTRAINT [DF_Accounts_UpdatedBy]	DEFAULT(''),
	[UpdatedUtcDate]					[datetime]					NOT NULL	CONSTRAINT [DF_Accounts_UpdatedUtcDate]	DEFAULT(GETUTCDATE()),
	[UpdatedUserAgent]						[nvarchar](200)				NOT NULL	CONSTRAINT [DF_Accounts_UpdatedUserAgent]	DEFAULT(''),
	CONSTRAINT Primary_Key_Accounts PRIMARY KEY CLUSTERED ([AccountId])
);

CREATE UNIQUE NONCLUSTERED INDEX
                [Accounts_1]
ON
[dbo].[Accounts] ([AccountName] ASC);
GO

INSERT [dbo].[Accounts] ([AccountId], [AccountName], [Description], [IsActive], [CreatedFromRemoteIpAddress], [CreatedBy], [CreatedUtcDate], [CreatedUserAgent], [UpdatedFromRemoteIpAddress], [UpdatedBy], [UpdatedUtcDate], [UpdatedUserAgent]) VALUES (N'84dac6c9-0920-45cb-ba47-4123d185f464', N'A place to demo features', N'For demos', 1, N'::1', N'CCL\valexander', CAST(N'2020-04-14T13:51:43.983' AS DateTime), N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36', N'::1', N'CCL\valexander', CAST(N'2020-04-14T13:51:43.983' AS DateTime), N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36')
GO

IF OBJECT_ID('dbo.EventEntries', 'U') IS NOT NULL
	DROP TABLE [dbo].[EventEntries];

CREATE TABLE [dbo].[EventEntries](
	[EventEntryId] [bigint] IDENTITY(1,1) NOT NULL,
	[StreamPath] [nvarchar](400) NOT NULL,
	[Data]					[varbinary](max)		NOT NULL,
	[DataType]				[nvarchar](400)		NOT NULL,
	[CompressionType]		[nvarchar](100)		NOT NULL,
	[SerializationType]			[nvarchar](100)		NOT NULL,
	[OccurredUtcDate] [datetime] NOT NULL,
	[NoticedUtcDate] [datetime] NOT NULL,
	[ReceivedUtcDate] [datetime] NOT NULL,
	[PersistedUtcDate] [datetime] NOT NULL	CONSTRAINT [DF_EventEntries_PersistedUtcDate]	DEFAULT(GETUTCDATE()),
	[CreatedBy]						[nvarchar](100)					NOT NULL	CONSTRAINT [DF_EventEntries_CreatedBy]	DEFAULT(''),
	[CreatedFromRemoteIpAddress]					[varchar](45)		NOT NULL	CONSTRAINT [DF_EventEntries_CreatedFromRemoteIpAddress]	DEFAULT(''),
	[CreatedUserAgent]						[nvarchar](200)				NOT NULL	CONSTRAINT [DF_EventEntries_CreatedUserAgent]	DEFAULT(''),
	CONSTRAINT [Primary_Key_EventEntries] PRIMARY KEY CLUSTERED ([EventEntryId])
 );
GO

CREATE NONCLUSTERED INDEX [EventEntries_1] ON [dbo].[EventEntries]
(
	[StreamPath] ASC
);
GO

