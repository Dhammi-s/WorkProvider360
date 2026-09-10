/* =============================================================================
   TENANT DATABASE — Meetings schema and stored procedures
   Run against EACH agency/tenant database.

   Fix applied 2026-09-10: FeePerParticipant was missing from usp_Meeting_Create
   and usp_Meeting_Update INSERT/UPDATE statements, causing NULL constraint
   violations when IsPaid = 1. All procedures below include the column.

   Column names returned by SELECTs intentionally match C# entity property
   names so Dapper maps them without configuration.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ═══════════════════════════════════════════════════════════════════════════
   SETTINGS
   ═══════════════════════════════════════════════════════════════════════════ */

CREATE OR ALTER PROCEDURE dbo.usp_MeetingSettings_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        AdminAccess, ManagerAccess, UserCanCreate, AllowClientParticipants,
        AllowPaidMeetings, DefaultFeePerParticipant, RequireApproval,
        NotifyOnCreate, NotifyOnUpdate, NotifyOnCancel,
        MaxParticipantsDefault, UpdatedOn
    FROM dbo.MeetingSettings;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_MeetingSettings_Update
    @AdminAccess              NVARCHAR(10),
    @ManagerAccess            NVARCHAR(10),
    @UserCanCreate            BIT,
    @AllowClientParticipants  BIT,
    @AllowPaidMeetings        BIT,
    @DefaultFeePerParticipant DECIMAL(18,2),
    @RequireApproval          BIT,
    @NotifyOnCreate           BIT,
    @NotifyOnUpdate           BIT,
    @NotifyOnCancel           BIT,
    @MaxParticipantsDefault   INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.MeetingSettings)
    BEGIN
        UPDATE dbo.MeetingSettings SET
            AdminAccess              = @AdminAccess,
            ManagerAccess            = @ManagerAccess,
            UserCanCreate            = @UserCanCreate,
            AllowClientParticipants  = @AllowClientParticipants,
            AllowPaidMeetings        = @AllowPaidMeetings,
            DefaultFeePerParticipant = @DefaultFeePerParticipant,
            RequireApproval          = @RequireApproval,
            NotifyOnCreate           = @NotifyOnCreate,
            NotifyOnUpdate           = @NotifyOnUpdate,
            NotifyOnCancel           = @NotifyOnCancel,
            MaxParticipantsDefault   = @MaxParticipantsDefault,
            UpdatedOn                = GETUTCDATE();
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MeetingSettings (
            AdminAccess, ManagerAccess, UserCanCreate, AllowClientParticipants,
            AllowPaidMeetings, DefaultFeePerParticipant, RequireApproval,
            NotifyOnCreate, NotifyOnUpdate, NotifyOnCancel,
            MaxParticipantsDefault, UpdatedOn
        ) VALUES (
            @AdminAccess, @ManagerAccess, @UserCanCreate, @AllowClientParticipants,
            @AllowPaidMeetings, @DefaultFeePerParticipant, @RequireApproval,
            @NotifyOnCreate, @NotifyOnUpdate, @NotifyOnCancel,
            @MaxParticipantsDefault, GETUTCDATE()
        );
    END

    SELECT TOP 1
        AdminAccess, ManagerAccess, UserCanCreate, AllowClientParticipants,
        AllowPaidMeetings, DefaultFeePerParticipant, RequireApproval,
        NotifyOnCreate, NotifyOnUpdate, NotifyOnCancel,
        MaxParticipantsDefault, UpdatedOn
    FROM dbo.MeetingSettings;
END
GO

/* ═══════════════════════════════════════════════════════════════════════════
   MEETINGS
   ═══════════════════════════════════════════════════════════════════════════ */

-- ─── CREATE ────────────────────────────────────────────────────────────────
-- FIX: @FeePerParticipant now included in parameter list AND INSERT statement.
CREATE OR ALTER PROCEDURE dbo.usp_Meeting_Create
    @Title             NVARCHAR(200),
    @Description       NVARCHAR(2000)  = NULL,
    @StartUtc          DATETIME2,
    @EndUtc            DATETIME2,
    @Location          NVARCHAR(500)   = NULL,
    @MeetingType       NVARCHAR(20)    = 'InPerson',
    @IsPaid            BIT             = 0,
    @FeePerParticipant DECIMAL(18,2)   = 0,
    @CreatedByUserId   INT,
    @MaxParticipants   INT             = NULL,
    @Notes             NVARCHAR(2000)  = NULL,
    @ColorTag          NVARCHAR(50)    = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Meetings (
        Title, Description, StartUtc, EndUtc, Location,
        MeetingType, Status, IsPaid, FeePerParticipant,
        CreatedByUserId, MaxParticipants, Notes, ColorTag,
        CreatedOn, UpdatedOn
    ) VALUES (
        @Title, @Description, @StartUtc, @EndUtc, @Location,
        @MeetingType, 'Scheduled', @IsPaid, ISNULL(@FeePerParticipant, 0),
        @CreatedByUserId, @MaxParticipants, @Notes, @ColorTag,
        GETUTCDATE(), GETUTCDATE()
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

-- ─── UPDATE ────────────────────────────────────────────────────────────────
-- FIX: @FeePerParticipant now included in parameter list AND UPDATE statement.
CREATE OR ALTER PROCEDURE dbo.usp_Meeting_Update
    @MeetingId         INT,
    @Title             NVARCHAR(200),
    @Description       NVARCHAR(2000)  = NULL,
    @StartUtc          DATETIME2,
    @EndUtc            DATETIME2,
    @Location          NVARCHAR(500)   = NULL,
    @MeetingType       NVARCHAR(20)    = 'InPerson',
    @IsPaid            BIT             = 0,
    @FeePerParticipant DECIMAL(18,2)   = 0,
    @MaxParticipants   INT             = NULL,
    @Notes             NVARCHAR(2000)  = NULL,
    @ColorTag          NVARCHAR(50)    = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Meetings SET
        Title             = @Title,
        Description       = @Description,
        StartUtc          = @StartUtc,
        EndUtc            = @EndUtc,
        Location          = @Location,
        MeetingType       = @MeetingType,
        IsPaid            = @IsPaid,
        FeePerParticipant = ISNULL(@FeePerParticipant, 0),
        MaxParticipants   = @MaxParticipants,
        Notes             = @Notes,
        ColorTag          = @ColorTag,
        UpdatedOn         = GETUTCDATE()
    WHERE MeetingId = @MeetingId;
END
GO

-- ─── UPDATE STATUS ─────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE dbo.usp_Meeting_UpdateStatus
    @MeetingId INT,
    @Status    NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Meetings
    SET Status = @Status, UpdatedOn = GETUTCDATE()
    WHERE MeetingId = @MeetingId;
END
GO

-- ─── DELETE ────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE dbo.usp_Meeting_Delete
    @MeetingId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.MeetingParticipants WHERE MeetingId = @MeetingId;
    DELETE FROM dbo.MeetingPayments      WHERE MeetingId = @MeetingId;
    DELETE FROM dbo.Meetings             WHERE MeetingId = @MeetingId;
END
GO

-- ─── GET BY ID ─────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE dbo.usp_Meeting_GetById
    @MeetingId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        m.MeetingId, m.Title, m.Description, m.StartUtc, m.EndUtc,
        m.Location, m.MeetingType, m.Status, m.IsPaid, m.FeePerParticipant,
        m.CreatedByUserId,
        u.FullName  AS CreatedByName,
        m.MaxParticipants, m.Notes, m.ColorTag,
        (SELECT COUNT(1) FROM dbo.MeetingParticipants WHERE MeetingId = m.MeetingId) AS ParticipantCount,
        m.CreatedOn, m.UpdatedOn
    FROM dbo.Meetings m
    LEFT JOIN dbo.Users u ON u.UserId = m.CreatedByUserId
    WHERE m.MeetingId = @MeetingId;
END
GO

-- ─── GET ALL ───────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE dbo.usp_Meeting_GetAll
    @FromUtc             DATETIME2   = NULL,
    @ToUtc               DATETIME2   = NULL,
    @Status              NVARCHAR(20)= NULL,
    @CreatedByUserId     INT         = NULL,
    @ParticipantUserId   INT         = NULL,
    @ParticipantClientId INT         = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        m.MeetingId, m.Title, m.Description, m.StartUtc, m.EndUtc,
        m.Location, m.MeetingType, m.Status, m.IsPaid, m.FeePerParticipant,
        m.CreatedByUserId,
        u.FullName AS CreatedByName,
        m.MaxParticipants, m.Notes, m.ColorTag,
        (SELECT COUNT(1) FROM dbo.MeetingParticipants WHERE MeetingId = m.MeetingId) AS ParticipantCount,
        m.CreatedOn, m.UpdatedOn
    FROM dbo.Meetings m
    LEFT JOIN dbo.Users u ON u.UserId = m.CreatedByUserId
    WHERE
        (@FromUtc             IS NULL OR m.StartUtc  >= @FromUtc)
        AND (@ToUtc           IS NULL OR m.EndUtc    <= @ToUtc)
        AND (@Status          IS NULL OR m.Status    = @Status)
        AND (@CreatedByUserId IS NULL OR m.CreatedByUserId = @CreatedByUserId)
        AND (@ParticipantUserId IS NULL OR m.MeetingId IN (
                SELECT MeetingId FROM dbo.MeetingParticipants WHERE UserId = @ParticipantUserId))
        AND (@ParticipantClientId IS NULL OR m.MeetingId IN (
                SELECT MeetingId FROM dbo.MeetingParticipants WHERE ClientId = @ParticipantClientId))
    ORDER BY m.StartUtc DESC;
END
GO

/* ═══════════════════════════════════════════════════════════════════════════
   PARTICIPANTS
   ═══════════════════════════════════════════════════════════════════════════ */

CREATE OR ALTER PROCEDURE dbo.usp_Meeting_AddParticipant
    @MeetingId       INT,
    @UserId          INT         = NULL,
    @ClientId        INT         = NULL,
    @ParticipantRole NVARCHAR(20) = 'Attendee'
AS
BEGIN
    SET NOCOUNT ON;

    -- Return -1 if already a participant
    IF EXISTS (
        SELECT 1 FROM dbo.MeetingParticipants
        WHERE MeetingId = @MeetingId
          AND ((@UserId   IS NOT NULL AND UserId   = @UserId)
            OR (@ClientId IS NOT NULL AND ClientId = @ClientId))
    )
    BEGIN
        SELECT -1;
        RETURN;
    END

    INSERT INTO dbo.MeetingParticipants (MeetingId, UserId, ClientId, ParticipantRole, Status, InvitedAt)
    VALUES (@MeetingId, @UserId, @ClientId, @ParticipantRole, 'Pending', GETUTCDATE());

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Meeting_GetParticipants
    @MeetingId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        mp.ParticipantId, mp.MeetingId, mp.UserId,
        u.FullName              AS ParticipantName,
        u.Email                 AS ParticipantEmail,
        r.RoleName              AS ParticipantRoleName,
        mp.ClientId,
        c.FirstName + ' ' + c.LastName AS ClientName,
        c.Email                 AS ClientEmail,
        mp.ParticipantRole, mp.Status, mp.IsPaid,
        mp.PaymentAmount, mp.PaymentDate, mp.PaymentMethod,
        mp.InvitedAt, mp.RespondedAt
    FROM dbo.MeetingParticipants mp
    LEFT JOIN dbo.Users    u ON u.UserId   = mp.UserId
    LEFT JOIN dbo.Roles    r ON r.RoleId   = u.RoleId
    LEFT JOIN dbo.Clients  c ON c.ClientId = mp.ClientId
    WHERE mp.MeetingId = @MeetingId
    ORDER BY mp.InvitedAt;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Meeting_RemoveParticipant
    @ParticipantId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.MeetingParticipants WHERE ParticipantId = @ParticipantId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Meeting_RespondParticipant
    @MeetingId INT,
    @UserId    INT,
    @Status    NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.MeetingParticipants
    SET Status = @Status, RespondedAt = GETUTCDATE()
    WHERE MeetingId = @MeetingId
      AND UserId    = @UserId
      AND ParticipantRole <> 'Host';

    SELECT @@ROWCOUNT;
END
GO

/* ═══════════════════════════════════════════════════════════════════════════
   PAYMENTS
   ═══════════════════════════════════════════════════════════════════════════ */

CREATE OR ALTER PROCEDURE dbo.usp_Meeting_RecordPayment
    @MeetingId        INT,
    @ParticipantId    INT,
    @Amount           DECIMAL(18,2),
    @Method           NVARCHAR(20),
    @Status           NVARCHAR(20)  = 'Paid',
    @TransactionId    NVARCHAR(200) = NULL,
    @Notes            NVARCHAR(500) = NULL,
    @RecordedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.MeetingPayments (
        MeetingId, ParticipantId, Amount, Method, Status,
        TransactionId, Notes, PaidAt, RecordedByUserId
    ) VALUES (
        @MeetingId, @ParticipantId, @Amount, @Method, @Status,
        @TransactionId, @Notes,
        CASE WHEN @Status = 'Paid' THEN GETUTCDATE() ELSE NULL END,
        @RecordedByUserId
    );

    -- Mark the participant as paid when payment status is 'Paid'
    IF @Status = 'Paid'
    BEGIN
        UPDATE dbo.MeetingParticipants
        SET IsPaid = 1, PaymentAmount = @Amount, PaymentDate = GETUTCDATE(), PaymentMethod = @Method
        WHERE ParticipantId = @ParticipantId;
    END

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Meeting_GetPayments
    @MeetingId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.PaymentId, p.MeetingId, p.ParticipantId,
        p.Amount, p.Method, p.Status, p.TransactionId,
        p.Notes, p.PaidAt, p.RecordedByUserId,
        u.FullName   AS RecordedByName,
        mp.UserId    AS ParticipantUserId,
        pu.FullName  AS ParticipantName,
        mp.ClientId  AS ParticipantClientId,
        c.FirstName + ' ' + c.LastName AS ParticipantClientName
    FROM dbo.MeetingPayments p
    LEFT JOIN dbo.Users              u  ON u.UserId    = p.RecordedByUserId
    LEFT JOIN dbo.MeetingParticipants mp ON mp.ParticipantId = p.ParticipantId
    LEFT JOIN dbo.Users              pu ON pu.UserId   = mp.UserId
    LEFT JOIN dbo.Clients            c  ON c.ClientId  = mp.ClientId
    WHERE p.MeetingId = @MeetingId
    ORDER BY p.PaidAt DESC;
END
GO
