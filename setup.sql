USE [master]
GO

CREATE DATABASE [clubsdb]
GO

USE [clubsdb]
GO

CREATE TABLE	[dbo].[Clubs]
(
	[Id]		[bigint]					IDENTITY(1,1) NOT NULL,
	[ClubId]	[nvarchar](64)				NOT NULL,
	[ClubName]	[nvarchar](128)				NOT NULL,
	CONSTRAINT	[PK_Clubs]					PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	),
	CONSTRAINT	[UK_Clubs_ClubId]			UNIQUE NONCLUSTERED
	(
		[ClubId] ASC
	),
	CONSTRAINT	[UK_Clubs_ClubName]			UNIQUE NONCLUSTERED
	(
		[ClubName] ASC
	)
)
GO

CREATE TABLE	[dbo].[PlayersInClub]
(
	[Id]		[bigint]					IDENTITY(1,1) NOT NULL,
	[PlayerId]	[bigint]					NOT NULL,
	[ClubId]	[nvarchar](64)				NOT NULL,
	CONSTRAINT	[PK_PlayersInClub]			PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	),
	CONSTRAINT	[UK_PlayersInClub_PlayerId]	UNIQUE NONCLUSTERED 
	(
		[PlayerId] ASC
	)
)
GO
