# Group Hands-On Laboratory Examination: Submission

GITHUB LINK: https://github.com/kinoroots/NotSeeGamers_midtermSUMMATIVELAB_elective

## Team NOTSEE G
| Member | Name | Assigned Role |
|---|---|---|
| Member 1 | BARRENO, ELIJAH | Systems Architect & Prompt Lead (Tasks 1, 5) |
| Member 2 | CALALANG, KYLE | Frontend Engineer (Task 2) |
| Member 3 | ALVAREZ, KAHLIL | Database & Backend Engineer (Task 3) |

## Task 1

### Prompt Used
task 1:
You are a Lead Systems Architect with 15 years of experience designing small web systems for universities.

CONTEXT: A team of 3-4 fourth-year BSIT students has 3 hours to build a working prototype of an Online Campus Event Management System. Students can view upcoming campus events and register for an event. Administrators can view the list of registered attendees per event. The team is beginner-level in generative AI tools. The prototype is graded on a frontend (semantic HTML, WCAG), a 3NF SQL Server database, a C# backend service, and unit tests.

TASK: Produce an overall system design covering: (1) high-level architecture and layers, (2) recommended tech stack, (3) main modules and their responsibilities, (4) core user flows for students and admins, (5) a folder structure with /frontend, /database, and /backend, and (6) a realistic 3-hour build order split across 3-4 people.

CONSTRAINTS:
- Do not use third-party state management libraries like Redux.
- Do not use frontend frameworks that need a build step (use plain HTML, CSS, and vanilla JS).
- Do not propose microservices, message queues, or cloud infrastructure.
- Do not use an ORM; use ADO.NET with parameterized queries.
- Keep the scope to what a student team can finish in 3 hours.
- Output in Markdown with clear headings, under 600 words.

task 2:
Act as a Frontend Engineer who specializes in accessible web design. Build a prototype interface for a Campus Event Catalog and Registration Form using plain HTML5, CSS, and vanilla JavaScript (no frameworks, no build step).

Requirements:
- Use semantic HTML5 tags only for structure: <header>, <main>, <section>, <article>, <footer>. Do not use generic <div> wrappers for page structure.
- Event Catalog: show 6 sample events as <article> cards (title, date, venue, seats left, short description, image with meaningful alt text).
- Registration Form: full name, student email, event dropdown, with a visible <label> linked to every input via for/id, plus aria-label on each input field, aria-required on required fields, and an accessible error message area (aria-live="polite").
- WCAG (POUR): color contrast of at least 4.5:1 for text, visible focus outlines, full keyboard navigation, a skip-to-content link, and a responsive layout.
- Client-side validation: email must end with @univ.edu.ph.

Constraints: no inline styles, no external CDN dependencies, no images without alt text. Output three separate files: index.html, styles.css, script.js.

task 3a:
Act as a Database Architect. Design a 3rd Normal Form (3NF) relational schema for an Online Campus Event Management System where students register for events and administrators view the attendees.

Requirements:
- At least 3 entities: Users, Events, Registrations (add others, like Venues or Roles, only if needed for 3NF).
- For each table, list columns, data types, primary key, foreign keys, and unique constraints.
- Briefly justify why the design is in 3NF (no partial or transitive dependencies).
- Prevent duplicate registrations (same user and event).
- Output an Entity-Relationship Diagram as a Mermaid.js erDiagram code block, with relationship cardinalities and PK/FK markers.

task 3b:
Using the schema above, generate a production-grade SQL Server (T-SQL) script named schema.sql.

Requirements:
- CREATE TABLE statements with primary keys (IDENTITY) and explicit, named FOREIGN KEY constraints with ON DELETE / ON UPDATE rules (explain each choice in a comment).
- CHECK constraints (e.g., capacity > 0, valid email format pattern, event end date after start date, valid role values).
- NOT NULL and UNIQUE constraints where appropriate.
- Explicit NONCLUSTERED indexes on every foreign key column.
- Make the script re-runnable (IF NOT EXISTS / DROP IF EXISTS guards) and add a few INSERT statements of sample data.
- Comment each section.
Output only the SQL script.

task 4a:
Act as a QA Engineer. Write C# unit tests using xUnit and Moq for a validation routine in a RegistrationService.

Cover:
1. Email domain validation: accept "name@univ.edu.ph"; reject other domains, empty/null input, and look-alike domains such as "name@univ.edu.ph.fake.com".
2. Seat availability: reject registration when the event is full, allow it when seats remain, and reject duplicate registration.

Constraints: use an IRegistrationRepository interface and mock it with Moq so there is no real database access. Use Arrange-Act-Assert and descriptive test names. Include the interface and the minimal service class needed to compile. Do not use real connections or static dependencies.

task 4b:
Act as a Application Security Reviewer. Diagnose the following C# method for (1) SQL injection risks and (2) unmanaged resource leaks. For each issue, give the exact line, why it's dangerous, and an example attack or failure scenario. Also flag any other bad practices (hardcoded credentials, ExecuteScalar null handling, SELECT *).

task 4c:
Refactor the method above into a RegistrationService class (RegistrationService.cs).

Requirements:
- Use a parameterized query (SqlParameter) instead of string concatenation.
- Wrap SqlConnection and SqlCommand in using statements.
- Select only the needed columns instead of SELECT *.
- Handle a null result from ExecuteScalar without throwing.
- Read the connection string from IConfiguration, not hardcoded.
- Validate that inputEmail is not null or empty.
Output only the C# file, with short comments explaining each fix.

task 5:
Act as a Technical Writer. Create a consolidated SUBMISSION.md with these sections:

1. Team Roster: table of members and assigned roles (Systems Architect & Prompt Lead, Frontend Engineer, Database & Backend Engineer, QA & Security Engineer).
2. Setup Instructions: how to run and view the frontend, how to run schema.sql in SQL Server, and how to build and run the unit tests.
3. AI Disclosure Statement: lists the AI tools used and how outputs were verified (manual code review, running tests, WCAG contrast checking, running the SQL script).
4. Group Verification Log: a table with columns Task # | Identified AI Flaw / Limitation | Manual Correction Applied | Member Responsible, with at least 3 rows.

Use this data: [paste member names/roles, tools used, and the real flaws you found]
Constraint: use only the information I provide and do not invent flaws or names.

### AI Output
<paste the full system design here>

### Manual Grounding Evaluation
<3-4 sentences, written by your team>

## Task 2: Frontend
We used Gemini to formulate this code.

## Task 3: Database
### ERD (Mermaid.js)
erDiagram
    ROLES {
        int role_id PK
        varchar role_name UK
    }
    USERS {
        int user_id PK
        varchar full_name
        varchar email UK
        int role_id FK
    }
    EVENTS {
        int event_id PK
        varchar title
        text description
        datetime event_date
        varchar venue
        int capacity
    }
    REGISTRATIONS {
        int registration_id PK
        int user_id FK
        int event_id FK
        datetime registered_at
    }

    ROLES ||--o{ USERS : "assigned"
    USERS ||--o{ REGISTRATIONS : "submits"
    EVENTS ||--o{ REGISTRATIONS : "receives"
    
Script location: `/database/schema.sql`

## Task 4: Testing & Security
- Unit tests: <where they are, what they cover, what is mocked>
- AI diagnosis of the flawed method: <paste summary>
- Refactored code: `/backend/RegistrationService.cs`

## Task 5: Setup Instructions
1. Clone the repo.
2. Run `/database/schema.sql` in SQL Server.
3. Run the backend: <command>
4. Open `/frontend/index.html` in a browser.

## AI Disclosure Statement
GEMINI
COPILOT
CLAUDE
