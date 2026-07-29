# C# & .NET Development — Internship Learning Journey

This repository documents my progress during my internship, where I've been learning C# and .NET development from the ground up over the past ~70 days.

## About

I'm a beginner developer working through a structured curriculum covering C# fundamentals, object-oriented programming, advanced C# features, database connectivity, and (coming up) web development, testing, and version control.

## Topics Covered So Far

### 1. Introduction to C# and .NET
- Overview of C# and .NET
- Setting up the development environment
- Writing and running simple C# programs
- Understanding CLR and JIT compilation

### 2. C# Basics
- Data types, variables, and constants
- Operators and expressions
- Control structures (if-else, switch-case, loops)
- Exception handling (try-catch-finally)
- Debugging techniques

### 3. Object-Oriented Programming (OOP)
- Classes and Objects
- Constructors and Destructors
- Inheritance and Polymorphism
- Abstraction and Encapsulation
- Interfaces and Abstract Classes

### 4. Advanced C# Features
- Collections (Lists, Dictionaries, Queues, Stacks)
- Generics and LINQ
- Delegates, Events, and Lambda Expressions
- Asynchronous Programming (async/await)
- File Handling

### 5. Database Connectivity with C# (in progress)
- Introduction to ADO.NET
- Connecting C# with SQL Server
- CRUD operations with SQL
- Using Entity Framework Core (EF Core) for ORM
- LINQ to SQL

## Featured Project: Student Manager

A console application built to practice database connectivity concepts. It implements the same CRUD operations two ways, side by side:

- **ADO.NET** — raw SQL queries with parameterized commands (SQL injection prevention)
- **EF Core** — using LINQ and a `DbContext`

**Tech used:**
- C# / .NET
- SQL Server Express LocalDB
- ADO.NET
- Entity Framework Core

**Project structure:**
```
StudentManager/
├── Models/         # Entity classes
├── Data/           # DbContext and data-access logic
├── Program.cs      # Application entry point
└── appsettings.json # Connection strings
```


## **Unit 6:**ASP.NET Core Web API — building RESTful APIs
Already Done Test using the Swagger and the internal https where I test the GET,GET by ID, POST ,DELETE,UPDATE,PATCH
## **Unit 7:** Testing & Debugging (NUnit/MSTest, logging)
I learn about the Logging and made a custom logging and also test the project I made and also debug using the exception and BreakPoint
## **Unit 8:** Git & GitHub collaboration workflows
I learn about the git init, git add . , git commit -m , git remote add (URL),git git git push, git branch like that .


## Coming Up Next
-We will see

## Notes

This repo will keep growing as I move through each unit — new folders/projects will be added as I build them out.