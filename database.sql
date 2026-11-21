IF DB_ID('Lab2DB') IS NULL CREATE DATABASE Lab2DB;
GO
USE Lab2DB;
GO

CREATE TABLE tblUsers (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(50) NOT NULL
);

CREATE TABLE tblDepartments (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL
);

CREATE TABLE tblInstructors (
    InstructorID INT IDENTITY(1,1) PRIMARY KEY,
    InstructorName NVARCHAR(100) NOT NULL,
    DepartmentID INT NOT NULL REFERENCES tblDepartments(DepartmentID)
);

CREATE TABLE tblCourses (
    CourseID INT IDENTITY(1,1) PRIMARY KEY,
    CourseName NVARCHAR(100) NOT NULL,
    Credits INT NOT NULL,
    DepartmentID INT NOT NULL REFERENCES tblDepartments(DepartmentID),
    InstructorID INT NOT NULL REFERENCES tblInstructors(InstructorID)
);

CREATE TABLE tblStudents (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentName NVARCHAR(100) NOT NULL,
    Age INT,
    YearOfEnroll INT,
    Major NVARCHAR(100),
    DateOfBirth DATE,
    GPA DECIMAL(3,2),
    City NVARCHAR(100)
);

CREATE TABLE tblEnrollments (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL REFERENCES tblStudents(StudentID) ON DELETE CASCADE,
    CourseID INT NOT NULL REFERENCES tblCourses(CourseID) ON DELETE CASCADE,
    UNIQUE (StudentID, CourseID)
);
GO

INSERT INTO tblUsers(Username, Password) VALUES ('user1', 'password1'), ('user2', 'password2');

INSERT INTO tblDepartments(DepartmentName) VALUES
    ('Computer Science'), ('Mathematics'), ('Physics'), ('Chemistry');

INSERT INTO tblInstructors(InstructorName, DepartmentID) VALUES
    ('Dr. John Smith', 1), ('Dr. Alice Johnson', 2), ('Dr. Nick Brown', 3), ('Dr. Bob White', 4);

INSERT INTO tblCourses(CourseName, Credits, DepartmentID, InstructorID) VALUES
    ('Introduction to CS', 3, 1, 1), ('Calculus I', 4, 2, 2),
    ('Physics I', 4, 3, 3), ('Organic Chemistry', 4, 4, 4);

INSERT INTO tblStudents(StudentName, Age, YearOfEnroll, Major, DateOfBirth, GPA, City) VALUES
    ('Trung Pham', 24, 2018, 'Computer Science', '2000-01-15', 3.50, 'HCMC'),
    ('Jane Smith', 25, 2017, 'Mathematics', '1999-05-22', 3.80, 'Los Angeles'),
    ('Alice Johnson', 23, 2019, 'Physics', '2001-03-10', 3.60, 'Chicago'),
    ('Bob Brown', 22, 2020, 'Chemistry', '2002-07-30', 3.70, 'Houston');

INSERT INTO tblEnrollments(StudentID, CourseID) VALUES (1, 1), (1, 2), (2, 2), (3, 3), (4, 4);
GO
