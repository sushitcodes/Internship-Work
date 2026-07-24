-- Drop old database if exists (start fresh!)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'TodoDB')
BEGIN
    DROP DATABASE TodoDB;
END
GO

-- Create new database
CREATE DATABASE TodoDB;
GO

-- Use the database
USE TodoDB;
GO

-- Create Todos table (simple!)
CREATE TABLE Todos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    IsCompleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Add sample data
INSERT INTO Todos (Title, Description, IsCompleted) VALUES 
('Learn C#', 'Complete the C# tutorial', 0),
('Build API', 'Create REST API with ASP.NET Core', 0),
('Test Swagger', 'Test all endpoints with Swagger', 1);
GO

-- Verify data
SELECT * FROM Todos;
GO