#  Program Evaluation & Analytics

Frontend for the K1 corporate training and employee evaluation platform. Built with Angular 18 and Angular Material.

## Features

| Area | Capabilities |
|---|---|
| **Program** | Create multi-week training programs with videos and assignments |
| **Assignments** | Define templates, distribute to employees, track submissions |
| **Evaluation** | Attendance, weekly evaluations, assessment scoring, qualitative feedback, leaderboard |
| **Identity** | Multi-tenant orgs, users, employees with role-based access |
| **Notifications** | Real-time via SignalR; email triggers for OTP and registration |
| **Auth** | Email + OTP sign-in with JWT access/refresh tokens |

## Architecture

```
src/app
├── core/              config, models, guards, interceptors, services, utils
├── shared/            shared components, pipes
├── layouts/           header, footer, main-layout
└── features/
    ├── auth/           login, OTP verification
    ├── dashboard/      landing page
    ├── identity/       organizations, users, employees
    ├── program/        program CRUD, weeks, videos, enrollments
    ├── assignment/     templates, employee assignments, submissions
    ├── evaluation/     attendance, weekly, assessment, feedback, leaderboard
    └── notification/   real-time + email notifications
```

## Getting started

```bash
npm install
npm start          # ng serve at http://localhost:4200
```

Set the gateway URL in `src/environments/environment.ts` (`gatewayUrl`). Defaults to `http://localhost:5000`.

## Build

```bash
npm run build              # development
npm run build:prod         # production
```

## Test

```bash
npm test                   # ng test
```
