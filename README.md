# Ethernet Terms Dictionary - HTTP Server

## Overview
This project is a backend server for managing an Ethernet terms dictionary.  
It provides a REST-like HTTP API to handle users, terms, categories, ratings, notes, and messaging features.  
The server is built with **.NET 8** and uses **MongoDB** for data storage.

---

## Features
- User registration and authentication with session tokens
- CRUD operations for terms
- Manage favorites and visited terms
- Categories retrieval
- Rate terms
- Add, update, and delete user notes
- Send messages and suggestions to other users/admins
- Logging and error handling

---

## Technologies
- **.NET 8** / C#
- **MongoDB** for database storage
- **ASP.NET Core Minimal API**
- **Serilog** for logging
- **JSON** for API communication

---

## Folder Structure
- **/user** – Handles user authentication, registration, and profile management.
- **/terms** – Handles CRUD operations for terms, visited terms, and favorites.
- **/categories** – Provides access to all term categories.
- **/rate** – Handles term ratings by users.
- **/notes** – Manages user notes for terms.
- **/messages** – Manages messages and suggestions between users and admins.

---

## API Overview
The server exposes the following endpoints:

### User
- `POST /api/user/auth` – Authenticate a user and receive a session token
- `POST /api/user/reg` – Register a new user
- `PUT /api/user` – Retrieve authenticated user data
- `GET /api/user` – Get all users

### Terms
- `GET /api/terms` – Retrieve all terms
- `GET /api/terms/visited` – Retrieve visited terms
- `POST /api/terms` – Add a new term
- `DELETE /api/terms` – Delete a term
- `POST /api/terms/like` – Add/remove term from favorites

### Categories
- `GET /api/categories` – Retrieve all term categories

### Rate
- `POST /api/rate` – Rate a term

### Notes
- `POST /api/note` – Add or update a note
- `DELETE /api/note` – Delete a note

### Messages
- `POST /api/message` – Send a message to another user
- `POST /api/message/suggestion` – Send a suggestion to admins
- `DELETE /api/message` – Delete all messages for the authenticated user

---

## Getting Started

1. Clone the repository:
```bash
git clone <repository_url>
```
2. Navigate to the project folder:
```bash
cd serverProject/server
```
3. Restore dependencies:
```bash
dotnet restore
```
4. 
```bash
dotnet run
```
The server will start at http://localhost:8888.

### Notes
- All requests and responses are JSON formatted.
- Authorization headers (Authorization: <token>) are required for most endpoints.
- Ensure MongoDB is running and accessible for the server to operate.

### Loggin
- Serilog is used for logging requests and errors.
- Logs can be written to console and/or rolling files.
