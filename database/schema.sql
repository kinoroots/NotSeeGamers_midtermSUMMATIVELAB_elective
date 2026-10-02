-- ============================================================================
-- Database Schema: Online Campus Event Management System
-- Engine: Microsoft SQL Server (T-SQL)
-- Description: Production-grade 3NF DDL script with constraints, indexes, and seed data.
-- ============================================================================

-- Safely drop existing tables in correct order (child to parent) to make script re-runnable
IF OBJECT_ID('dbo.Registrations', 'U') IS NOT NULL DROP TABLE dbo.Registrations;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;

-- ============================================================================
-- 1. ROLES TABLE
-- ============================================================================
CREATE TABLE dbo.Roles (
    role_id INT IDENTITY(1,1) NOT NULL,
    role_name VARCHAR(50) NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (role_id),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (role_name),
    -- Ensure only valid system roles are permitted
    CONSTRAINT CK_Roles_ValidName CHECK (role_name IN ('Student', 'Administrator', 'Faculty'))
);

-- ============================================================================
-- 2. USERS TABLE
-- ============================================================================
CREATE TABLE dbo.Users (
    user_id INT IDENTITY(1,1) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    role_id INT NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (user_id),
    CONSTRAINT UQ_Users_Email UNIQUE (email),
    -- Enforce university email domain validation rule
    CONSTRAINT CK_Users_EmailDomain CHECK (email LIKE '%@univ.edu.ph'),
    -- Foreign Key: Users belong to a Role. 
    -- ON UPDATE CASCADE: If role_id changes, propagate to user records.
    -- ON DELETE NO ACTION: Prevent deleting a role if user accounts are still assigned to it.
    CONSTRAINT FK_Users_Roles FOREIGN KEY (role_id) 
        REFERENCES dbo.Roles(role_id) 
        ON UPDATE CASCADE 
        ON DELETE NO ACTION
);

-- ============================================================================
-- 3. EVENTS TABLE
-- ============================================================================
CREATE TABLE dbo.Events (
    event_id INT IDENTITY(1,1) NOT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT NULL,
    event_date DATETIME NOT NULL,
    venue VARCHAR(100) NOT NULL,
    capacity INT NOT NULL,
    CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (event_id),
    -- Ensure event capacity is always a positive integer greater than zero
    CONSTRAINT CK_Events_Capacity CHECK (capacity > 0)
);

-- ============================================================================
-- 4. REGISTRATIONS TABLE (Junction Table for Many-to-Many Relationship)
-- ============================================================================
CREATE TABLE dbo.Registrations (
    registration_id INT IDENTITY(1,1) NOT NULL,
    user_id INT NOT NULL,
    event_id INT NOT NULL,
    registered_at DATETIME NOT NULL CONSTRAINT DF_Registrations_RegisteredAt DEFAULT GETDATE(),
    CONSTRAINT PK_Registrations PRIMARY KEY CLUSTERED (registration_id),
    -- Prevent duplicate registrations: A specific user cannot register for the same event more than once
    CONSTRAINT UQ_User_Event UNIQUE (user_id, event_id),
    -- Foreign Key: Links registration to User.
    -- ON DELETE CASCADE: If a user account is deleted, remove their event registrations automatically.
    CONSTRAINT FK_Registrations_Users FOREIGN KEY (user_id) 
        REFERENCES dbo.Users(user_id) 
        ON UPDATE CASCADE 
        ON DELETE CASCADE,
    -- Foreign Key: Links registration to Event.
    -- ON DELETE CASCADE: If an event is removed, clean up associated registrations automatically.
    CONSTRAINT FK_Registrations_Events FOREIGN KEY (event_id) 
        REFERENCES dbo.Events(event_id) 
        ON UPDATE CASCADE 
        ON DELETE CASCADE
);

-- ============================================================================
-- 5. PERFORMANCE INDEXES (Nonclustered Indexes on Foreign Key Columns)
-- ============================================================================
CREATE NONCLUSTERED INDEX IX_Users_role_id 
ON dbo.Users (role_id);

CREATE NONCLUSTERED INDEX IX_Registrations_user_id 
ON dbo.Registrations (user_id);

CREATE NONCLUSTERED INDEX IX_Registrations_event_id 
ON dbo.Registrations (event_id);

-- ============================================================================
-- 6. SAMPLE SEED DATA
-- ============================================================================
-- Insert Default Roles
INSERT INTO dbo.Roles (role_name) VALUES 
('Student'), 
('Administrator');

-- Insert Sample Users
INSERT INTO dbo.Users (full_name, email, role_id) VALUES 
('Elijah Barreno', 'elibarreno@univ.edu.ph', 1),
('Shantel De Guzman', 'sdeguzman@univ.edu.ph', 1),
('Kyle Calalang', 'kcalalang@univ.edu.ph', 1),
('System Administrator', 'admin@univ.edu.ph', 2);

-- Insert Sample Events
INSERT INTO dbo.Events (title, description, event_date, venue, capacity) VALUES 
('Applied Generative AI Tech Summit', 'Exploring modern LLM frameworks and prompt engineering workflows for IT solutions.', '2026-06-15 09:00:00', 'Main Auditorium', 120),
('Cisco Networking & Security Workshop', 'Hands-on session covering OSPF routing configurations and VLAN management.', '2026-06-20 13:00:00', 'Computer Laboratory 302', 45);

-- Insert Sample Registrations
INSERT INTO dbo.Registrations (user_id, event_id) VALUES 
(1, 1), -- Elijah registered for Tech Summit
(2, 1), -- Shantel registered for Tech Summit
(1, 2); -- Elijah registered for Cisco Workshop