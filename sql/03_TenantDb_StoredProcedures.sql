/* =============================================================================
   TENANT DATABASE — stored procedures
   Run against EACH agency/tenant database, after 02_TenantDb_Schema.sql.

   Column names returned by the SELECTs intentionally match the C# entity
   property names so Dapper maps them without configuration.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ------------------------------- Roles ----------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Role_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, IsActive FROM dbo.Roles ORDER BY RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Role_GetById
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, IsActive FROM dbo.Roles WHERE RoleId = @RoleId;
END
GO

/* ------------------------------- Users ----------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserId, u.Email, u.FullName, u.PasswordHash, u.PasswordSalt,
        u.RoleId, r.RoleName, u.IsActive, u.CreatedOn, u.UpdatedOn
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
    WHERE u.Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserId, u.Email, u.FullName, u.PasswordHash, u.PasswordSalt,
        u.RoleId, r.RoleName, u.IsActive, u.CreatedOn, u.UpdatedOn
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
    WHERE u.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserId, u.Email, u.FullName, u.PasswordHash, u.PasswordSalt,
        u.RoleId, r.RoleName, u.IsActive, u.CreatedOn, u.UpdatedOn
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
    ORDER BY u.UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_EmailExists
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM dbo.Users WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_Create
    @Email        NVARCHAR(256),
    @FullName     NVARCHAR(200),
    @PasswordHash NVARCHAR(200),
    @PasswordSalt NVARCHAR(100),
    @RoleId       INT,
    @IsActive     BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users (Email, FullName, PasswordHash, PasswordSalt, RoleId, IsActive)
    VALUES (@Email, @FullName, @PasswordHash, @PasswordSalt, @RoleId, @IsActive);

    SELECT CAST(SCOPE_IDENTITY() AS INT);   -- new UserId
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_UpdatePassword
    @UserId       INT,
    @PasswordHash NVARCHAR(200),
    @PasswordSalt NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
        SET PasswordHash = @PasswordHash,
            PasswordSalt = @PasswordSalt,
            UpdatedOn    = SYSUTCDATETIME()
    WHERE UserId = @UserId;
END
GO

/* --------------------------- RefreshTokens ------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_RefreshToken_Create
    @UserId    INT,
    @Token     NVARCHAR(200),
    @ExpiresOn DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.RefreshTokens (UserId, Token, ExpiresOn)
    VALUES (@UserId, @Token, @ExpiresOn);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshToken_GetActive
    @UserId INT,
    @Token  NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RefreshTokenId, UserId, Token, ExpiresOn, IsRevoked, CreatedOn
    FROM dbo.RefreshTokens
    WHERE UserId = @UserId
      AND Token = @Token
      AND IsRevoked = 0
      AND ExpiresOn > SYSUTCDATETIME();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshToken_Revoke
    @RefreshTokenId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.RefreshTokens SET IsRevoked = 1 WHERE RefreshTokenId = @RefreshTokenId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshToken_RevokeAllForUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.RefreshTokens SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0;
END
GO

/* ------------------------ PasswordResetTokens ---------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_PasswordResetToken_Create
    @UserId    INT,
    @Token     NVARCHAR(200),
    @ExpiresOn DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PasswordResetTokens (UserId, Token, ExpiresOn)
    VALUES (@UserId, @Token, @ExpiresOn);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_PasswordResetToken_GetActive
    @UserId INT,
    @Token  NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PasswordResetTokenId, UserId, Token, ExpiresOn, IsUsed, CreatedOn
    FROM dbo.PasswordResetTokens
    WHERE UserId = @UserId
      AND Token = @Token
      AND IsUsed = 0
      AND ExpiresOn > SYSUTCDATETIME();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_PasswordResetToken_MarkUsed
    @PasswordResetTokenId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PasswordResetTokens SET IsUsed = 1 WHERE PasswordResetTokenId = @PasswordResetTokenId;
END
GO
