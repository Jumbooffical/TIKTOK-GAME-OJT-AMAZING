import aiosqlite
from datetime import datetime

DB_PATH = "tiktok_live.db"

CREATE_TABLES_SQL = """
CREATE TABLE IF NOT EXISTS sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL,
    room_id TEXT,
    started_at TEXT DEFAULT (datetime('now', 'localtime')),
    ended_at TEXT,
    total_viewers INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS comments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER REFERENCES sessions(id),
    user_id TEXT,
    username TEXT,
    nickname TEXT,
    comment TEXT,
    timestamp TEXT DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS gifts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER REFERENCES sessions(id),
    user_id TEXT,
    username TEXT,
    nickname TEXT,
    gift_name TEXT,
    gift_id TEXT,
    diamond_count INTEGER DEFAULT 0,
    repeat_count INTEGER DEFAULT 1,
    timestamp TEXT DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS joins (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER REFERENCES sessions(id),
    user_id TEXT,
    username TEXT,
    nickname TEXT,
    timestamp TEXT DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS likes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER REFERENCES sessions(id),
    user_id TEXT,
    username TEXT,
    nickname TEXT,
    like_count INTEGER DEFAULT 0,
    total_likes INTEGER DEFAULT 0,
    timestamp TEXT DEFAULT (datetime('now', 'localtime'))
);
"""


async def init_db():
    async with aiosqlite.connect(DB_PATH) as db:
        for statement in CREATE_TABLES_SQL.strip().split(";"):
            s = statement.strip()
            if s:
                await db.execute(s)
        await db.commit()


async def create_session(username: str, room_id: str = None) -> int:
    async with aiosqlite.connect(DB_PATH) as db:
        cursor = await db.execute(
            "INSERT INTO sessions (username, room_id) VALUES (?, ?)",
            (username, room_id),
        )
        await db.commit()
        return cursor.lastrowid


async def end_session(session_id: int):
    async with aiosqlite.connect(DB_PATH) as db:
        await db.execute(
            "UPDATE sessions SET ended_at = datetime('now','localtime') WHERE id = ?",
            (session_id,),
        )
        await db.commit()


async def save_comment(session_id: int, data: dict):
    async with aiosqlite.connect(DB_PATH) as db:
        await db.execute(
            """INSERT INTO comments (session_id, user_id, username, nickname, comment, timestamp)
               VALUES (?, ?, ?, ?, ?, ?)""",
            (
                session_id,
                data.get("user_id"),
                data.get("username"),
                data.get("nickname"),
                data.get("comment"),
                data.get("timestamp"),
            ),
        )
        await db.commit()


async def save_gift(session_id: int, data: dict):
    async with aiosqlite.connect(DB_PATH) as db:
        await db.execute(
            """INSERT INTO gifts (session_id, user_id, username, nickname, gift_name, gift_id, diamond_count, repeat_count, timestamp)
               VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)""",
            (
                session_id,
                data.get("user_id"),
                data.get("username"),
                data.get("nickname"),
                data.get("gift_name"),
                data.get("gift_id"),
                data.get("diamond_count", 0),
                data.get("repeat_count", 1),
                data.get("timestamp"),
            ),
        )
        await db.commit()


async def save_join(session_id: int, data: dict):
    async with aiosqlite.connect(DB_PATH) as db:
        await db.execute(
            """INSERT INTO joins (session_id, user_id, username, nickname, timestamp)
               VALUES (?, ?, ?, ?, ?)""",
            (
                session_id,
                data.get("user_id"),
                data.get("username"),
                data.get("nickname"),
                data.get("timestamp"),
            ),
        )
        await db.commit()


async def save_like(session_id: int, data: dict):
    async with aiosqlite.connect(DB_PATH) as db:
        await db.execute(
            """INSERT INTO likes (session_id, user_id, username, nickname, like_count, total_likes, timestamp)
               VALUES (?, ?, ?, ?, ?, ?, ?)""",
            (
                session_id,
                data.get("user_id"),
                data.get("username"),
                data.get("nickname"),
                data.get("like_count", 0),
                data.get("total_likes", 0),
                data.get("timestamp"),
            ),
        )
        await db.commit()


async def get_sessions():
    async with aiosqlite.connect(DB_PATH) as db:
        db.row_factory = aiosqlite.Row
        cursor = await db.execute(
            "SELECT * FROM sessions ORDER BY started_at DESC"
        )
        rows = await cursor.fetchall()
        return [dict(r) for r in rows]


async def get_session_comments(session_id: int, limit: int = 500):
    async with aiosqlite.connect(DB_PATH) as db:
        db.row_factory = aiosqlite.Row
        cursor = await db.execute(
            "SELECT * FROM comments WHERE session_id = ? ORDER BY timestamp DESC LIMIT ?",
            (session_id, limit),
        )
        rows = await cursor.fetchall()
        return [dict(r) for r in rows]


async def get_session_gifts(session_id: int, limit: int = 500):
    async with aiosqlite.connect(DB_PATH) as db:
        db.row_factory = aiosqlite.Row
        cursor = await db.execute(
            "SELECT * FROM gifts WHERE session_id = ? ORDER BY timestamp DESC LIMIT ?",
            (session_id, limit),
        )
        rows = await cursor.fetchall()
        return [dict(r) for r in rows]


async def get_session_joins(session_id: int, limit: int = 500):
    async with aiosqlite.connect(DB_PATH) as db:
        db.row_factory = aiosqlite.Row
        cursor = await db.execute(
            "SELECT * FROM joins WHERE session_id = ? ORDER BY timestamp DESC LIMIT ?",
            (session_id, limit),
        )
        rows = await cursor.fetchall()
        return [dict(r) for r in rows]


async def get_session_stats(session_id: int):
    async with aiosqlite.connect(DB_PATH) as db:
        db.row_factory = aiosqlite.Row

        async def scalar(sql, params=()):
            cur = await db.execute(sql, params)
            row = await cur.fetchone()
            return row[0] if row else 0

        total_comments = await scalar(
            "SELECT COUNT(*) FROM comments WHERE session_id = ?", (session_id,)
        )
        total_gifts = await scalar(
            "SELECT COUNT(*) FROM gifts WHERE session_id = ?", (session_id,)
        )
        total_diamonds = await scalar(
            "SELECT COALESCE(SUM(diamond_count * repeat_count), 0) FROM gifts WHERE session_id = ?",
            (session_id,),
        )
        total_likes = await scalar(
            "SELECT COALESCE(MAX(total_likes), 0) FROM likes WHERE session_id = ?",
            (session_id,),
        )
        total_joins = await scalar(
            "SELECT COUNT(*) FROM joins WHERE session_id = ?", (session_id,)
        )

        cur = await db.execute(
            """SELECT nickname, username,
                      SUM(diamond_count * repeat_count) as total_diamonds,
                      COUNT(*) as gift_count
               FROM gifts WHERE session_id = ?
               GROUP BY user_id
               ORDER BY total_diamonds DESC
               LIMIT 10""",
            (session_id,),
        )
        top_gifters = [dict(r) for r in await cur.fetchall()]

        cur = await db.execute(
            "SELECT * FROM sessions WHERE id = ?", (session_id,)
        )
        row = await cur.fetchone()
        session_info = dict(row) if row else {}

        return {
            "session": session_info,
            "total_comments": total_comments,
            "total_gifts": total_gifts,
            "total_diamonds": total_diamonds,
            "total_likes": total_likes,
            "total_joins": total_joins,
            "top_gifters": top_gifters,
        }
