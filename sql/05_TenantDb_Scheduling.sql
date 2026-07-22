/* =============================================================================
   TENANT DATABASE — scheduling feature (tables + stored procedures)
   Run against EACH agency/tenant database, after 02 + 03 (+ 04) scripts.

   Tables:
     SchedulingSettings  — single-row config: role access levels + pay defaults
     Schedules           — one row per scheduled job / shift assigned to a user
     ScheduleNotes       — user/admin notes and injury reports against a schedule
     TimeEntries         — clock in/out and manual worked-time records

   Access levels (SchedulingSettings.AdminAccess / ManagerAccess):
     'None'  — role cannot see the scheduler at all
     'Read'  — role can view schedules + reports, but not create/edit
     'Write' — role can create/edit/assign schedules and edit defaults
   SuperAdmin is always full-write and ignores these settings.

   Column names returned by the SELECTs intentionally match the C# entity
   property names so Dapper maps them without configuration.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   SchedulingSettings — a single row (SettingsId = 1) holds access + defaults.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.SchedulingSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchedulingSettings
    (
        SettingsId                INT            NOT NULL CONSTRAINT PK_SchedulingSettings PRIMARY KEY,
        AdminAccess               NVARCHAR(10)   NOT NULL CONSTRAINT DF_SchedSettings_AdminAccess   DEFAULT (N'Write'),
        ManagerAccess             NVARCHAR(10)   NOT NULL CONSTRAINT DF_SchedSettings_ManagerAccess DEFAULT (N'Read'),
        DefaultPayRatePerHour     DECIMAL(10,2)  NOT NULL CONSTRAINT DF_SchedSettings_PayRate       DEFAULT (0),
        DefaultOvertimeMultiplier DECIMAL(5,2)   NOT NULL CONSTRAINT DF_SchedSettings_OtMultiplier  DEFAULT (1.5),
        NotifyAdminOnCreate       BIT            NOT NULL CONSTRAINT DF_SchedSettings_NotifyAdmin   DEFAULT (0),
        NotifyManagerOnCreate     BIT            NOT NULL CONSTRAINT DF_SchedSettings_NotifyManager DEFAULT (0),
        UpdatedOn                 DATETIME2(7)   NOT NULL CONSTRAINT DF_SchedSettings_UpdatedOn     DEFAULT (SYSUTCDATETIME())
    );
END
GO

/* Ensure the single settings row exists. */
IF NOT EXISTS (SELECT 1 FROM dbo.SchedulingSettings WHERE SettingsId = 1)
BEGIN
    INSERT INTO dbo.SchedulingSettings (SettingsId) VALUES (1);
END
GO

/* ---------------------------------------------------------------------------
   Schedules — one row per scheduled job assigned to a user.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Schedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Schedules
    (
        ScheduleId         INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Schedules PRIMARY KEY,
        Title              NVARCHAR(200)  NOT NULL,
        CustomerName       NVARCHAR(200)  NULL,
        Location           NVARCHAR(300)  NULL,
        AssignedUserId     INT            NOT NULL,
        StartUtc           DATETIME2(7)   NOT NULL,
        EndUtc             DATETIME2(7)   NOT NULL,
        PayRatePerHour     DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Schedules_PayRate      DEFAULT (0),
        OvertimeMultiplier DECIMAL(5,2)   NOT NULL CONSTRAINT DF_Schedules_OtMultiplier DEFAULT (1.5),
        Status             NVARCHAR(20)   NOT NULL CONSTRAINT DF_Schedules_Status       DEFAULT (N'Scheduled'),
        RejectionReason    NVARCHAR(1000) NULL,
        ColorTag           NVARCHAR(20)   NULL,
        CreatedByUserId    INT            NOT NULL,
        CreatedOn          DATETIME2(7)   NOT NULL CONSTRAINT DF_Schedules_CreatedOn    DEFAULT (SYSUTCDATETIME()),
        UpdatedOn          DATETIME2(7)   NOT NULL CONSTRAINT DF_Schedules_UpdatedOn    DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Schedules_AssignedUser FOREIGN KEY (AssignedUserId)  REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_Schedules_CreatedBy    FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_Schedules_AssignedUserId ON dbo.Schedules (AssignedUserId);
    CREATE INDEX IX_Schedules_StartUtc       ON dbo.Schedules (StartUtc);
END
GO

/* ---------------------------------------------------------------------------
   ScheduleNotes — notes + injury reports. NoteType in ('Note','Injury').
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ScheduleNotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScheduleNotes
    (
        NoteId       INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScheduleNotes PRIMARY KEY,
        ScheduleId   INT            NOT NULL,
        AuthorUserId INT            NOT NULL,
        NoteType     NVARCHAR(20)   NOT NULL CONSTRAINT DF_ScheduleNotes_Type DEFAULT (N'Note'),
        Message      NVARCHAR(2000) NOT NULL,
        CreatedOn    DATETIME2(7)   NOT NULL CONSTRAINT DF_ScheduleNotes_CreatedOn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_ScheduleNotes_Schedules FOREIGN KEY (ScheduleId)   REFERENCES dbo.Schedules (ScheduleId),
        CONSTRAINT FK_ScheduleNotes_Author    FOREIGN KEY (AuthorUserId) REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_ScheduleNotes_ScheduleId ON dbo.ScheduleNotes (ScheduleId);
END
GO

/* ---------------------------------------------------------------------------
   TimeEntries — clock in/out + manual worked-time. Source in ('Timer','Manual').
   ClockOutUtc is NULL while a timer is still running.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.TimeEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TimeEntries
    (
        TimeEntryId INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_TimeEntries PRIMARY KEY,
        ScheduleId  INT            NOT NULL,
        UserId      INT            NOT NULL,
        ClockInUtc  DATETIME2(7)   NOT NULL,
        ClockOutUtc DATETIME2(7)   NULL,
        Source      NVARCHAR(10)   NOT NULL CONSTRAINT DF_TimeEntries_Source DEFAULT (N'Timer'),
        Note        NVARCHAR(500)  NULL,
        CreatedOn   DATETIME2(7)   NOT NULL CONSTRAINT DF_TimeEntries_CreatedOn DEFAULT (SYSUTCDATETIME()),
        UpdatedOn   DATETIME2(7)   NOT NULL CONSTRAINT DF_TimeEntries_UpdatedOn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_TimeEntries_Schedules FOREIGN KEY (ScheduleId) REFERENCES dbo.Schedules (ScheduleId),
        CONSTRAINT FK_TimeEntries_Users     FOREIGN KEY (UserId)     REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_TimeEntries_ScheduleId ON dbo.TimeEntries (ScheduleId);
    CREATE INDEX IX_TimeEntries_UserId     ON dbo.TimeEntries (UserId);
END
GO

/* =============================== PROCEDURES ================================= */

/* ------------------------- SchedulingSettings ---------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_SchedulingSettings_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettingsId, AdminAccess, ManagerAccess, DefaultPayRatePerHour,
           DefaultOvertimeMultiplier, NotifyAdminOnCreate, NotifyManagerOnCreate, UpdatedOn
    FROM dbo.SchedulingSettings
    WHERE SettingsId = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SchedulingSettings_UpdateAccess
    @AdminAccess   NVARCHAR(10),
    @ManagerAccess NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.SchedulingSettings WHERE SettingsId = 1)
        INSERT INTO dbo.SchedulingSettings (SettingsId) VALUES (1);

    UPDATE dbo.SchedulingSettings
    SET AdminAccess = @AdminAccess,
        ManagerAccess = @ManagerAccess,
        UpdatedOn = SYSUTCDATETIME()
    WHERE SettingsId = 1;

    EXEC dbo.usp_SchedulingSettings_Get;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SchedulingSettings_UpdateDefaults
    @DefaultPayRatePerHour     DECIMAL(10,2),
    @DefaultOvertimeMultiplier DECIMAL(5,2),
    @NotifyAdminOnCreate       BIT,
    @NotifyManagerOnCreate     BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.SchedulingSettings WHERE SettingsId = 1)
        INSERT INTO dbo.SchedulingSettings (SettingsId) VALUES (1);

    UPDATE dbo.SchedulingSettings
    SET DefaultPayRatePerHour = @DefaultPayRatePerHour,
        DefaultOvertimeMultiplier = @DefaultOvertimeMultiplier,
        NotifyAdminOnCreate = @NotifyAdminOnCreate,
        NotifyManagerOnCreate = @NotifyManagerOnCreate,
        UpdatedOn = SYSUTCDATETIME()
    WHERE SettingsId = 1;

    EXEC dbo.usp_SchedulingSettings_Get;
END
GO

/* ------------------------------ Schedules -------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Schedule_Create
    @Title              NVARCHAR(200),
    @CustomerName       NVARCHAR(200) = NULL,
    @Location           NVARCHAR(300) = NULL,
    @AssignedUserId     INT,
    @StartUtc           DATETIME2(7),
    @EndUtc             DATETIME2(7),
    @PayRatePerHour     DECIMAL(10,2),
    @OvertimeMultiplier DECIMAL(5,2),
    @ColorTag           NVARCHAR(20)  = NULL,
    @CreatedByUserId    INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Schedules
        (Title, CustomerName, Location, AssignedUserId, StartUtc, EndUtc,
         PayRatePerHour, OvertimeMultiplier, Status, ColorTag, CreatedByUserId)
    VALUES
        (@Title, @CustomerName, @Location, @AssignedUserId, @StartUtc, @EndUtc,
         @PayRatePerHour, @OvertimeMultiplier, N'Scheduled', @ColorTag, @CreatedByUserId);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedule_GetAll
    @FromUtc        DATETIME2(7) = NULL,
    @ToUtc          DATETIME2(7) = NULL,
    @AssignedUserId INT          = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.ScheduleId, s.Title, s.CustomerName, s.Location,
        s.AssignedUserId, u.FullName AS AssignedUserName,
        s.StartUtc, s.EndUtc, s.PayRatePerHour, s.OvertimeMultiplier,
        s.Status, s.RejectionReason, s.ColorTag,
        s.CreatedByUserId, s.CreatedOn, s.UpdatedOn
    FROM dbo.Schedules s
    INNER JOIN dbo.Users u ON u.UserId = s.AssignedUserId
    WHERE (@FromUtc IS NULL OR s.EndUtc   >= @FromUtc)
      AND (@ToUtc   IS NULL OR s.StartUtc <  @ToUtc)
      AND (@AssignedUserId IS NULL OR s.AssignedUserId = @AssignedUserId)
    ORDER BY s.StartUtc;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedule_GetById
    @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.ScheduleId, s.Title, s.CustomerName, s.Location,
        s.AssignedUserId, u.FullName AS AssignedUserName,
        s.StartUtc, s.EndUtc, s.PayRatePerHour, s.OvertimeMultiplier,
        s.Status, s.RejectionReason, s.ColorTag,
        s.CreatedByUserId, s.CreatedOn, s.UpdatedOn
    FROM dbo.Schedules s
    INNER JOIN dbo.Users u ON u.UserId = s.AssignedUserId
    WHERE s.ScheduleId = @ScheduleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedule_Update
    @ScheduleId         INT,
    @Title              NVARCHAR(200),
    @CustomerName       NVARCHAR(200) = NULL,
    @Location           NVARCHAR(300) = NULL,
    @AssignedUserId     INT,
    @StartUtc           DATETIME2(7),
    @EndUtc             DATETIME2(7),
    @PayRatePerHour     DECIMAL(10,2),
    @OvertimeMultiplier DECIMAL(5,2),
    @ColorTag           NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Schedules
    SET Title = @Title,
        CustomerName = @CustomerName,
        Location = @Location,
        AssignedUserId = @AssignedUserId,
        StartUtc = @StartUtc,
        EndUtc = @EndUtc,
        PayRatePerHour = @PayRatePerHour,
        OvertimeMultiplier = @OvertimeMultiplier,
        ColorTag = @ColorTag,
        UpdatedOn = SYSUTCDATETIME()
    WHERE ScheduleId = @ScheduleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedule_UpdateStatus
    @ScheduleId      INT,
    @Status          NVARCHAR(20),
    @RejectionReason NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Schedules
    SET Status = @Status,
        RejectionReason = @RejectionReason,
        UpdatedOn = SYSUTCDATETIME()
    WHERE ScheduleId = @ScheduleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedule_Delete
    @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.TimeEntries   WHERE ScheduleId = @ScheduleId;
    DELETE FROM dbo.ScheduleNotes WHERE ScheduleId = @ScheduleId;
    DELETE FROM dbo.Schedules     WHERE ScheduleId = @ScheduleId;
END
GO

/* ----------------------------- ScheduleNotes ----------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_ScheduleNote_Create
    @ScheduleId   INT,
    @AuthorUserId INT,
    @NoteType     NVARCHAR(20),
    @Message      NVARCHAR(2000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ScheduleNotes (ScheduleId, AuthorUserId, NoteType, Message)
    VALUES (@ScheduleId, @AuthorUserId, @NoteType, @Message);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ScheduleNote_GetBySchedule
    @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT n.NoteId, n.ScheduleId, n.AuthorUserId, u.FullName AS AuthorName,
           n.NoteType, n.Message, n.CreatedOn
    FROM dbo.ScheduleNotes n
    INNER JOIN dbo.Users u ON u.UserId = n.AuthorUserId
    WHERE n.ScheduleId = @ScheduleId
    ORDER BY n.CreatedOn;
END
GO

/* ------------------------------ TimeEntries ------------------------------ */
CREATE OR ALTER PROCEDURE dbo.usp_TimeEntry_ClockIn
    @ScheduleId INT,
    @UserId     INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TimeEntries (ScheduleId, UserId, ClockInUtc, Source)
    VALUES (@ScheduleId, @UserId, SYSUTCDATETIME(), N'Timer');

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TimeEntry_ClockOut
    @ScheduleId INT,
    @UserId     INT
AS
BEGIN
    SET NOCOUNT ON;
    /* Close the most recent still-open timer for this user + schedule. */
    UPDATE te
    SET te.ClockOutUtc = SYSUTCDATETIME(),
        te.UpdatedOn = SYSUTCDATETIME()
    FROM dbo.TimeEntries te
    INNER JOIN (
        SELECT TOP (1) TimeEntryId
        FROM dbo.TimeEntries
        WHERE ScheduleId = @ScheduleId AND UserId = @UserId AND ClockOutUtc IS NULL
        ORDER BY ClockInUtc DESC
    ) open_entry ON open_entry.TimeEntryId = te.TimeEntryId;

    SELECT @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TimeEntry_GetOpen
    @ScheduleId INT,
    @UserId     INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) TimeEntryId, ScheduleId, UserId, ClockInUtc, ClockOutUtc, Source, Note, CreatedOn, UpdatedOn
    FROM dbo.TimeEntries
    WHERE ScheduleId = @ScheduleId AND UserId = @UserId AND ClockOutUtc IS NULL
    ORDER BY ClockInUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TimeEntry_Create
    @ScheduleId  INT,
    @UserId      INT,
    @ClockInUtc  DATETIME2(7),
    @ClockOutUtc DATETIME2(7) = NULL,
    @Note        NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TimeEntries (ScheduleId, UserId, ClockInUtc, ClockOutUtc, Source, Note)
    VALUES (@ScheduleId, @UserId, @ClockInUtc, @ClockOutUtc, N'Manual', @Note);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TimeEntry_Update
    @TimeEntryId INT,
    @ClockInUtc  DATETIME2(7),
    @ClockOutUtc DATETIME2(7) = NULL,
    @Note        NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TimeEntries
    SET ClockInUtc = @ClockInUtc,
        ClockOutUtc = @ClockOutUtc,
        Note = @Note,
        UpdatedOn = SYSUTCDATETIME()
    WHERE TimeEntryId = @TimeEntryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TimeEntry_GetBySchedule
    @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT te.TimeEntryId, te.ScheduleId, te.UserId, u.FullName AS UserName,
           te.ClockInUtc, te.ClockOutUtc, te.Source, te.Note, te.CreatedOn, te.UpdatedOn
    FROM dbo.TimeEntries te
    INNER JOIN dbo.Users u ON u.UserId = te.UserId
    WHERE te.ScheduleId = @ScheduleId
    ORDER BY te.ClockInUtc;
END
GO

/* --------------------------------- Report -------------------------------- */
/* One row per schedule in range, with total COMPLETED worked seconds. The BLL
   layer computes regular vs overtime hours and earnings from these rows. */
CREATE OR ALTER PROCEDURE dbo.usp_Schedule_GetReport
    @FromUtc        DATETIME2(7),
    @ToUtc          DATETIME2(7),
    @AssignedUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.ScheduleId,
        s.Title,
        s.AssignedUserId,
        u.FullName AS AssignedUserName,
        s.StartUtc,
        s.EndUtc,
        s.PayRatePerHour,
        s.OvertimeMultiplier,
        s.Status,
        ISNULL((
            SELECT SUM(DATEDIFF(SECOND, te.ClockInUtc, te.ClockOutUtc))
            FROM dbo.TimeEntries te
            WHERE te.ScheduleId = s.ScheduleId AND te.ClockOutUtc IS NOT NULL
        ), 0) AS WorkedSeconds
    FROM dbo.Schedules s
    INNER JOIN dbo.Users u ON u.UserId = s.AssignedUserId
    WHERE s.StartUtc >= @FromUtc
      AND s.StartUtc <  @ToUtc
      AND (@AssignedUserId IS NULL OR s.AssignedUserId = @AssignedUserId)
    ORDER BY u.FullName, s.StartUtc;
END
GO
