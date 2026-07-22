/* =============================================================================
   TENANT DATABASE — live location tracking (table + stored procedures)
   Run against EACH agency/tenant database, after 05 (scheduling) script.

   LocationPings holds periodic GPS positions recorded by the assigned user's
   device WHILE THEY ARE CLOCKED IN on a schedule. Used to show a live map and
   route breadcrumb to authorised managers/admins for client + worker safety.

   Column names returned by the SELECTs intentionally match the C# entity
   property names so Dapper maps them without configuration.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.LocationPings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LocationPings
    (
        PingId         BIGINT         IDENTITY(1,1) NOT NULL CONSTRAINT PK_LocationPings PRIMARY KEY,
        ScheduleId     INT            NOT NULL,
        UserId         INT            NOT NULL,
        Latitude       DECIMAL(9,6)   NOT NULL,
        Longitude      DECIMAL(9,6)   NOT NULL,
        AccuracyMeters DECIMAL(9,2)   NULL,
        RecordedUtc    DATETIME2(7)   NOT NULL CONSTRAINT DF_LocationPings_RecordedUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_LocationPings_Schedules FOREIGN KEY (ScheduleId) REFERENCES dbo.Schedules (ScheduleId),
        CONSTRAINT FK_LocationPings_Users     FOREIGN KEY (UserId)     REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_LocationPings_Schedule ON dbo.LocationPings (ScheduleId, RecordedUtc);
    CREATE INDEX IX_LocationPings_User     ON dbo.LocationPings (UserId, RecordedUtc);
END
GO

/* =============================== PROCEDURES ================================= */

CREATE OR ALTER PROCEDURE dbo.usp_LocationPing_Create
    @ScheduleId     INT,
    @UserId         INT,
    @Latitude       DECIMAL(9,6),
    @Longitude      DECIMAL(9,6),
    @AccuracyMeters DECIMAL(9,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LocationPings (ScheduleId, UserId, Latitude, Longitude, AccuracyMeters)
    VALUES (@ScheduleId, @UserId, @Latitude, @Longitude, @AccuracyMeters);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END
GO

/* Full breadcrumb trail for one schedule, oldest first. */
CREATE OR ALTER PROCEDURE dbo.usp_LocationPing_GetTrail
    @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.PingId, p.ScheduleId, p.UserId, u.FullName AS UserName,
           p.Latitude, p.Longitude, p.AccuracyMeters, p.RecordedUtc
    FROM dbo.LocationPings p
    INNER JOIN dbo.Users u ON u.UserId = p.UserId
    WHERE p.ScheduleId = @ScheduleId
    ORDER BY p.RecordedUtc;
END
GO

/* Latest position for every schedule that is currently "live" (the assigned
   user has an open time entry). Optionally scoped to a single user. */
CREATE OR ALTER PROCEDURE dbo.usp_LocationPing_GetLiveLatest
    @UserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.ScheduleId,
        s.Title,
        s.AssignedUserId AS UserId,
        u.FullName       AS UserName,
        s.CustomerName,
        s.Location,
        p.Latitude,
        p.Longitude,
        p.AccuracyMeters,
        p.RecordedUtc
    FROM dbo.Schedules s
    INNER JOIN dbo.Users u ON u.UserId = s.AssignedUserId
    CROSS APPLY (
        SELECT TOP (1) lp.Latitude, lp.Longitude, lp.AccuracyMeters, lp.RecordedUtc
        FROM dbo.LocationPings lp
        WHERE lp.ScheduleId = s.ScheduleId
        ORDER BY lp.RecordedUtc DESC
    ) p
    WHERE EXISTS (
        SELECT 1 FROM dbo.TimeEntries te
        WHERE te.ScheduleId = s.ScheduleId AND te.ClockOutUtc IS NULL
    )
    AND (@UserId IS NULL OR s.AssignedUserId = @UserId)
    ORDER BY p.RecordedUtc DESC;
END
GO
