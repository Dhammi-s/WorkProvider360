/* =============================================================================
   TENANT DATABASE — schema + static seed data
   Run this against EACH agency/tenant database (e.g. db43502).

   Tables:
     Roles                 — static role catalog (ids fixed across all tenants)
     Users                 — application users (salted SHA-512 passwords)
     RefreshTokens         — issued refresh tokens (rotated on use)
     PasswordResetTokens   — single-use forgot-password tokens
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   Roles — ids are STATIC and must match SaaS.Core.Constants.RoleConstants.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId   INT           NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        RoleName NVARCHAR(50)  NOT NULL,
        IsActive BIT           NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
        CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
    );
END
GO

/* Seed / keep the static roles in sync (id 1..4). */
MERGE dbo.Roles AS target
USING (VALUES
    (1, N'SuperAdmin'),
    (2, N'Admin'),
    (3, N'Manager'),
    (4, N'User')
) AS source (RoleId, RoleName)
ON target.RoleId = source.RoleId
WHEN MATCHED THEN UPDATE SET target.RoleName = source.RoleName
WHEN NOT MATCHED THEN INSERT (RoleId, RoleName, IsActive) VALUES (source.RoleId, source.RoleName, 1);
GO

/* ---------------------------------------------------------------------------
   Users
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId       INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Email        NVARCHAR(256)  NOT NULL,
        FullName     NVARCHAR(200)  NOT NULL,
        PasswordHash NVARCHAR(200)  NOT NULL,   -- base64 SHA-512
        PasswordSalt NVARCHAR(100)  NOT NULL,   -- base64 salt
        RoleId       INT            NOT NULL,
        IsActive     BIT            NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedOn    DATETIME2(7)   NOT NULL CONSTRAINT DF_Users_CreatedOn DEFAULT (SYSUTCDATETIME()),
        UpdatedOn    DATETIME2(7)   NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId)
    );
END
GO

/* ---------------------------------------------------------------------------
   RefreshTokens
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        RefreshTokenId BIGINT       IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId         INT          NOT NULL,
        Token          NVARCHAR(200) NOT NULL,
        ExpiresOn      DATETIME2(7) NOT NULL,
        IsRevoked      BIT          NOT NULL CONSTRAINT DF_RefreshTokens_IsRevoked DEFAULT (0),
        CreatedOn      DATETIME2(7) NOT NULL CONSTRAINT DF_RefreshTokens_CreatedOn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_RefreshTokens_UserId_Token ON dbo.RefreshTokens (UserId, Token);
END
GO

/* ---------------------------------------------------------------------------
   PasswordResetTokens
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        PasswordResetTokenId BIGINT       IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        UserId               INT          NOT NULL,
        Token                NVARCHAR(200) NOT NULL,
        ExpiresOn            DATETIME2(7) NOT NULL,
        IsUsed               BIT          NOT NULL CONSTRAINT DF_PasswordResetTokens_IsUsed DEFAULT (0),
        CreatedOn            DATETIME2(7) NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedOn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_PasswordResetTokens_UserId_Token ON dbo.PasswordResetTokens (UserId, Token);
END
GO
