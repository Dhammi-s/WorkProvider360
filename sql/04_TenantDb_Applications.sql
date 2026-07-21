/* =============================================================================
   TENANT DATABASE — role-application workflow (tables + stored procedures)
   Run against EACH agency/tenant database, after 02 + 03 scripts.

   Tables:
     ApplicationSettings   — single-row config for the public application form
     ApplicationQuestions  — SuperAdmin-defined custom screening questions
     RoleApplications      — submitted requests for Admin/Manager access
     ApplicationAnswers    — applicant answers to the custom questions

   Column names returned by the SELECTs intentionally match the C# entity
   property names so Dapper maps them without configuration.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   ApplicationSettings — a single row (SettingsId = 1) holds form configuration.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ApplicationSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationSettings
    (
        SettingsId                INT           NOT NULL CONSTRAINT PK_ApplicationSettings PRIMARY KEY,
        RequirePhone              BIT           NOT NULL CONSTRAINT DF_AppSettings_RequirePhone   DEFAULT (1),
        RequireAddress            BIT           NOT NULL CONSTRAINT DF_AppSettings_RequireAddress DEFAULT (1),
        EmailNotificationsEnabled BIT           NOT NULL CONSTRAINT DF_AppSettings_Email          DEFAULT (1),
        NotificationEmail         NVARCHAR(256) NULL,
        UpdatedOn                 DATETIME2(7)  NOT NULL CONSTRAINT DF_AppSettings_UpdatedOn      DEFAULT (SYSUTCDATETIME())
    );
END
GO

/* Ensure the single settings row exists. */
IF NOT EXISTS (SELECT 1 FROM dbo.ApplicationSettings WHERE SettingsId = 1)
BEGIN
    INSERT INTO dbo.ApplicationSettings (SettingsId, RequirePhone, RequireAddress, EmailNotificationsEnabled)
    VALUES (1, 1, 1, 1);
END
GO

/* ---------------------------------------------------------------------------
   ApplicationQuestions — custom questions rendered on the public form.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ApplicationQuestions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationQuestions
    (
        QuestionId   INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationQuestions PRIMARY KEY,
        QuestionText NVARCHAR(500)  NOT NULL,
        IsRequired   BIT            NOT NULL CONSTRAINT DF_AppQuestions_IsRequired DEFAULT (1),
        IsActive     BIT            NOT NULL CONSTRAINT DF_AppQuestions_IsActive   DEFAULT (1),
        SortOrder    INT            NOT NULL CONSTRAINT DF_AppQuestions_SortOrder  DEFAULT (0),
        CreatedOn    DATETIME2(7)   NOT NULL CONSTRAINT DF_AppQuestions_CreatedOn  DEFAULT (SYSUTCDATETIME())
    );
END
GO

/* ---------------------------------------------------------------------------
   RoleApplications — one row per submitted application.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.RoleApplications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoleApplications
    (
        ApplicationId    INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_RoleApplications PRIMARY KEY,
        FullName         NVARCHAR(200)  NOT NULL,
        Email            NVARCHAR(256)  NOT NULL,
        Phone            NVARCHAR(50)   NULL,
        Address          NVARCHAR(500)  NULL,
        RequestedRoleId  INT            NOT NULL,
        Status           NVARCHAR(20)   NOT NULL CONSTRAINT DF_RoleApplications_Status DEFAULT (N'Pending'),
        RejectionReason  NVARCHAR(1000) NULL,
        ReviewedByUserId INT            NULL,
        ReviewedOn       DATETIME2(7)   NULL,
        CreatedOn        DATETIME2(7)   NOT NULL CONSTRAINT DF_RoleApplications_CreatedOn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_RoleApplications_Roles FOREIGN KEY (RequestedRoleId) REFERENCES dbo.Roles (RoleId)
    );
    CREATE INDEX IX_RoleApplications_Status ON dbo.RoleApplications (Status);
END
GO

/* ---------------------------------------------------------------------------
   ApplicationAnswers — answers to custom questions (question text snapshotted).
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ApplicationAnswers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationAnswers
    (
        AnswerId      INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationAnswers PRIMARY KEY,
        ApplicationId INT            NOT NULL,
        QuestionId    INT            NULL,
        QuestionText  NVARCHAR(500)  NOT NULL,
        AnswerText    NVARCHAR(2000) NULL,
        CONSTRAINT FK_ApplicationAnswers_Applications FOREIGN KEY (ApplicationId) REFERENCES dbo.RoleApplications (ApplicationId)
    );
    CREATE INDEX IX_ApplicationAnswers_ApplicationId ON dbo.ApplicationAnswers (ApplicationId);
END
GO

/* =============================== PROCEDURES ================================= */

/* ------------------------- ApplicationSettings --------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_ApplicationSettings_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettingsId, RequirePhone, RequireAddress, EmailNotificationsEnabled, NotificationEmail, UpdatedOn
    FROM dbo.ApplicationSettings
    WHERE SettingsId = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationSettings_Upsert
    @RequirePhone              BIT,
    @RequireAddress            BIT,
    @EmailNotificationsEnabled BIT,
    @NotificationEmail         NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ApplicationSettings WHERE SettingsId = 1)
    BEGIN
        UPDATE dbo.ApplicationSettings
        SET RequirePhone = @RequirePhone,
            RequireAddress = @RequireAddress,
            EmailNotificationsEnabled = @EmailNotificationsEnabled,
            NotificationEmail = @NotificationEmail,
            UpdatedOn = SYSUTCDATETIME()
        WHERE SettingsId = 1;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.ApplicationSettings (SettingsId, RequirePhone, RequireAddress, EmailNotificationsEnabled, NotificationEmail)
        VALUES (1, @RequirePhone, @RequireAddress, @EmailNotificationsEnabled, @NotificationEmail);
    END

    SELECT SettingsId, RequirePhone, RequireAddress, EmailNotificationsEnabled, NotificationEmail, UpdatedOn
    FROM dbo.ApplicationSettings
    WHERE SettingsId = 1;
END
GO

/* ------------------------- ApplicationQuestions -------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_ApplicationQuestion_GetActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT QuestionId, QuestionText, IsRequired, IsActive, SortOrder, CreatedOn
    FROM dbo.ApplicationQuestions
    WHERE IsActive = 1
    ORDER BY SortOrder, QuestionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationQuestion_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT QuestionId, QuestionText, IsRequired, IsActive, SortOrder, CreatedOn
    FROM dbo.ApplicationQuestions
    ORDER BY SortOrder, QuestionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationQuestion_GetById
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT QuestionId, QuestionText, IsRequired, IsActive, SortOrder, CreatedOn
    FROM dbo.ApplicationQuestions
    WHERE QuestionId = @QuestionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationQuestion_Create
    @QuestionText NVARCHAR(500),
    @IsRequired   BIT,
    @SortOrder    INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ApplicationQuestions (QuestionText, IsRequired, IsActive, SortOrder)
    VALUES (@QuestionText, @IsRequired, 1, @SortOrder);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationQuestion_Update
    @QuestionId   INT,
    @QuestionText NVARCHAR(500),
    @IsRequired   BIT,
    @IsActive     BIT,
    @SortOrder    INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ApplicationQuestions
    SET QuestionText = @QuestionText,
        IsRequired = @IsRequired,
        IsActive = @IsActive,
        SortOrder = @SortOrder
    WHERE QuestionId = @QuestionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationQuestion_Deactivate
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ApplicationQuestions SET IsActive = 0 WHERE QuestionId = @QuestionId;
END
GO

/* --------------------------- RoleApplications ---------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_RoleApplication_Create
    @FullName        NVARCHAR(200),
    @Email           NVARCHAR(256),
    @Phone           NVARCHAR(50)  = NULL,
    @Address         NVARCHAR(500) = NULL,
    @RequestedRoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.RoleApplications (FullName, Email, Phone, Address, RequestedRoleId, Status)
    VALUES (@FullName, @Email, @Phone, @Address, @RequestedRoleId, N'Pending');

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RoleApplication_GetAll
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.ApplicationId, a.FullName, a.Email, a.Phone, a.Address,
        a.RequestedRoleId, r.RoleName AS RequestedRoleName,
        a.Status, a.RejectionReason, a.ReviewedByUserId, a.ReviewedOn, a.CreatedOn
    FROM dbo.RoleApplications a
    INNER JOIN dbo.Roles r ON r.RoleId = a.RequestedRoleId
    WHERE (@Status IS NULL OR a.Status = @Status)
    ORDER BY a.CreatedOn DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RoleApplication_GetById
    @ApplicationId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.ApplicationId, a.FullName, a.Email, a.Phone, a.Address,
        a.RequestedRoleId, r.RoleName AS RequestedRoleName,
        a.Status, a.RejectionReason, a.ReviewedByUserId, a.ReviewedOn, a.CreatedOn
    FROM dbo.RoleApplications a
    INNER JOIN dbo.Roles r ON r.RoleId = a.RequestedRoleId
    WHERE a.ApplicationId = @ApplicationId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RoleApplication_UpdateStatus
    @ApplicationId    INT,
    @Status           NVARCHAR(20),
    @RejectionReason  NVARCHAR(1000) = NULL,
    @ReviewedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.RoleApplications
    SET Status = @Status,
        RejectionReason = @RejectionReason,
        ReviewedByUserId = @ReviewedByUserId,
        ReviewedOn = SYSUTCDATETIME()
    WHERE ApplicationId = @ApplicationId;
END
GO

/* --------------------------- ApplicationAnswers -------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_ApplicationAnswer_Create
    @ApplicationId INT,
    @QuestionId    INT = NULL,
    @QuestionText  NVARCHAR(500),
    @AnswerText    NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ApplicationAnswers (ApplicationId, QuestionId, QuestionText, AnswerText)
    VALUES (@ApplicationId, @QuestionId, @QuestionText, @AnswerText);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplicationAnswer_GetByApplication
    @ApplicationId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AnswerId, ApplicationId, QuestionId, QuestionText, AnswerText
    FROM dbo.ApplicationAnswers
    WHERE ApplicationId = @ApplicationId
    ORDER BY AnswerId;
END
GO
