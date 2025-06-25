# Modern Blogging Platform

A full-featured blogging platform built with Angular frontend and ASP.NET Core backend.

## Features

- User Authentication (signup, login, password reset)
- Article Management (create, edit, delete blog posts)
- Rich Text Editing
- Article Categorization
- Comment System
- Search Functionality

## Technology Stack

### Frontend
- Angular
- TypeScript
- Angular Toast for notifications
- Angular Material for UI components

### Backend
- ASP.NET Core (C#)
- Entity Framework Core
- SQL Server

## Project Structure

- `/BloggingPlatform.API` - ASP.NET Core backend
- `/blogging-platform-ui` - Angular frontend

## Getting Started

### Prerequisites

- Node.js and npm
- .NET 6 SDK or later
- SQL Server
- Angular CLI

### Setup Instructions

#### Backend
1. Navigate to the `BloggingPlatform.API` directory
2. Update the connection string in `appsettings.json`
3. Run `dotnet restore`
4. Run `dotnet ef database update` to create the database
5. Run `dotnet run` to start the API

#### Frontend
1. Navigate to the `blogging-platform-ui` directory
2. Run `npm install`
3. Update the API URL in `environment.ts`
4. Run `ng serve` to start the development server
5. Open your browser to `http://localhost:4200`