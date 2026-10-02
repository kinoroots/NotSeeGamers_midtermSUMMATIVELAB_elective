INSERT INTO dbo.Events (title, description, event_date, venue, capacity)
SELECT catalog.title, catalog.description, catalog.event_date, catalog.venue, catalog.capacity
FROM (VALUES
    ('Welcome Back Picnic', 'Lawn games, local snacks, and good company under the old maples.', CAST('2026-09-18T12:00:00' AS DATETIME), 'Maple Quad', 24),
    ('Open-Air Jazz Night', 'Student jazz, sunset skies, and late-summer air.', CAST('2026-09-23T18:30:00' AS DATETIME), 'Amphitheater', 38),
    ('Mindful Garden Walk', 'A guided lakeside garden walk with breathing and tea.', CAST('2026-10-02T08:00:00' AS DATETIME), 'University Garden', 12),
    ('Printmaking for Beginners', 'A low-pressure introduction to hand printing; materials provided.', CAST('2026-10-09T15:00:00' AS DATETIME), 'Arts Studio 204', 8),
    ('Stories at the Stacks', 'Share a favorite short story and a simple meal with readers.', CAST('2026-10-16T17:30:00' AS DATETIME), 'Hawthorne Library', 19),
    ('Night Sky Social', 'Meet the astronomy club, find constellations, and share hot chocolate.', CAST('2026-10-22T19:00:00' AS DATETIME), 'North Observatory', 16)
) AS catalog(title, description, event_date, venue, capacity)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Events AS existing
    WHERE existing.title = catalog.title
);