/* =============================================================================
   MASTER DATABASE — stored procedures
   Run this against the master database that holds the [Agencies] table.

   The Agencies table already exists with these columns:
     AgencyId, AgencyName, DomainUrl, Location, DbServer, DbName, DbUser,
     DbPassword, ConnectionString, IsActive, IsArchived, CreatedOn, UpdatedOn
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* Resolve a tenant by domain. The application sends a normalized host such as
   "yourdomain.com"; this proc strips scheme/slashes from the stored DomainUrl
   so it matches regardless of how the URL was saved. */
CREATE OR ALTER PROCEDURE dbo.usp_Agency_GetByDomain
    @DomainUrl NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        AgencyId, AgencyName, DomainUrl, Location, DbServer, DbName, DbUser,
        DbPassword, ConnectionString, IsActive, IsArchived, CreatedOn, UpdatedOn
    FROM dbo.Agencies
    WHERE IsActive = 1
      AND IsArchived = 0
      AND REPLACE(REPLACE(REPLACE(REPLACE(DomainUrl,
              'https://', ''), 'http://', ''), '/', ''), ' ', '') = @DomainUrl;
END
GO

/* Resolve a tenant by its primary key (used for authenticated requests where
   the agency id travels inside the JWT). */
CREATE OR ALTER PROCEDURE dbo.usp_Agency_GetById
    @AgencyId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AgencyId, AgencyName, DomainUrl, Location, DbServer, DbName, DbUser,
        DbPassword, ConnectionString, IsActive, IsArchived, CreatedOn, UpdatedOn
    FROM dbo.Agencies
    WHERE AgencyId = @AgencyId;
END
GO
