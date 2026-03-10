# Comments SPA Project

This is a **Comments System** with Angular frontend and ASP.NET Core backend.  
Supports HTML tags, file uploads (images & text), CAPTCHA, pagination, and threaded replies.

---

## Features

- Submit comments with:
  - Text with allowed HTML tags: `<a>`, `<i>`, `<strong>`, `<code>`
  - Image files (`jpg`, `jpeg`, `png`, `gif`)
  - Text files (`.txt` up to 100 KB)
- Threaded replies
- CAPTCHA validation
- Pagination and sorting
- Preview comments without sending
- Safe HTML rendering with links opening in a new tab
- Client-side validation
- AJAX-based communication between frontend and backend
- Optional visual effects and formatting toolbar

---

## Technologies

- **Frontend**: Angular 21.2.1, TypeScript, HTML, CSS
- **Backend**: NET 10, ASP.NET Core 10.0.3
- **Database**: MS SQL Server
- **Others**: Docker support for full-stack deployment

## Setup

### 1. Clone repository in folder and start 

```bash
git clone https://github.com/Vynogradov-Mykola/comments-spa/
cd path/to/comments-spa
docker compose up --build
```

Page available on http://localhost:4200/
