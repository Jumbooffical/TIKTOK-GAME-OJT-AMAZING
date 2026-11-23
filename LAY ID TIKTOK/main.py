import asyncio
import json
import logging
from pathlib import Path

import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, HTTPException
from fastapi.responses import HTMLResponse

import database as db
from tiktok_client import TikTokManager

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)
logger = logging.getLogger(__name__)

app = FastAPI(title="TikTok Live Dashboard")
unity_app = FastAPI(title="TikTok Live Unity Server")


# ─── WebSocket Connection Manager ─────────────────────────────────────────────

class ConnectionManager:
    def __init__(self):
        self.active: list[WebSocket] = []

    async def connect(self, ws: WebSocket):
        await ws.accept()
        self.active.append(ws)

    def disconnect(self, ws: WebSocket):
        if ws in self.active:
            self.active.remove(ws)

    async def broadcast(self, data: dict):
        dead = []
        for ws in self.active:
            try:
                await ws.send_text(json.dumps(data, ensure_ascii=False))
            except Exception:
                dead.append(ws)
        for ws in dead:
            self.disconnect(ws)


ws_manager = ConnectionManager()
tiktok = TikTokManager(ws_manager, db)


# ─── Startup ──────────────────────────────────────────────────────────────────

@app.on_event("startup")
async def startup():
    await db.init_db()
    logger.info("✅ Database initialized — tiktok_live.db")


# ─── REST API ─────────────────────────────────────────────────────────────────

@app.post("/api/connect")
async def connect_live(payload: dict):
    username = (payload.get("username") or "").strip()
    if not username:
        raise HTTPException(status_code=400, detail="Thiếu username")
    try:
        await tiktok.start(username)
        return {"ok": True, "username": username}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/disconnect")
async def disconnect_live():
    session_id = await tiktok.stop()
    return {"ok": True, "session_id": session_id}


@app.get("/api/status")
async def get_status():
    return tiktok.get_status()


@app.get("/api/sessions")
async def list_sessions():
    return await db.get_sessions()


@app.get("/api/session/{session_id}/comments")
async def session_comments(session_id: int, limit: int = 500):
    return await db.get_session_comments(session_id, limit)


@app.get("/api/session/{session_id}/gifts")
async def session_gifts(session_id: int, limit: int = 500):
    return await db.get_session_gifts(session_id, limit)


@app.get("/api/session/{session_id}/joins")
async def session_joins(session_id: int, limit: int = 500):
    return await db.get_session_joins(session_id, limit)


@app.get("/api/session/{session_id}/stats")
async def session_stats(session_id: int):
    return await db.get_session_stats(session_id)


# ─── WebSocket ────────────────────────────────────────────────────────────────

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await ws_manager.connect(websocket)
    try:
        while True:
            await websocket.receive_text()
    except WebSocketDisconnect:
        ws_manager.disconnect(websocket)


# Second WebSocket port: 8001
@unity_app.websocket("/ws")
async def websocket_endpoint_2(websocket: WebSocket):
    await ws_manager.connect(websocket)
    try:
        while True:
            await websocket.receive_text()
    except WebSocketDisconnect:
        ws_manager.disconnect(websocket)


# ─── Serve Frontend ───────────────────────────────────────────────────────────

@app.get("/", response_class=HTMLResponse)
async def get_index():
    html_path = Path(__file__).parent / "frontend" / "index.html"
    return HTMLResponse(html_path.read_text(encoding="utf-8"))


async def run_servers():

    # Main API + WebSocket server
    config_main = uvicorn.Config(
        app,
        host="127.0.0.1",
        port=8000,
        log_level="info"
    )

    # Second Unity-only server
    config_unity = uvicorn.Config(
        unity_app,
        host="127.0.0.1",
        port=8001,
        log_level="info"
    )

    server_main = uvicorn.Server(config_main)
    server_unity = uvicorn.Server(config_unity)

    # Let the main server handle Ctrl+C
    server_unity.install_signal_handlers = lambda: None

    # Start second server in the background
    ws_task = asyncio.create_task(server_unity.serve())

    try:
        # Run main server
        await server_main.serve()

    finally:
        # Stop both servers together
        server_unity.should_exit = True
        await ws_task


if __name__ == "__main__":
    asyncio.run(run_servers())
