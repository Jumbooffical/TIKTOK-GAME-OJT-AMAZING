import asyncio
import logging
from datetime import datetime
from typing import Optional

logger = logging.getLogger(__name__)


class TikTokManager:
    def __init__(self, ws_manager, db_module):
        self.client = None
        self.ws_manager = ws_manager
        self.db = db_module
        self.session_id: Optional[int] = None
        self.username: Optional[str] = None
        self.connected = False
        self._task: Optional[asyncio.Task] = None

    def _now(self) -> str:
        return datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    async def start(self, username: str):
        if self.connected or self._task:
            await self.stop()

        self.username = username.lstrip("@")
        self.session_id = await self.db.create_session(self.username)
        self.connected = False

        # Báo frontend xóa toàn bộ dữ liệu cũ (trừ tab lịch sử)
        await self.ws_manager.broadcast({
            "type": "clear",
            "username": self.username,
            "session_id": self.session_id,
            "timestamp": self._now(),
        })

        try:
            from TikTokLive import TikTokLiveClient
            from TikTokLive.events import (
                CommentEvent,
                GiftEvent,
                LikeEvent,
                JoinEvent,
                ConnectEvent,
                DisconnectEvent,
                LiveEndEvent,
            )

            self.client = TikTokLiveClient(unique_id=f"@{self.username}")

            # ── Connect ──────────────────────────────────
            @self.client.on(ConnectEvent)
            async def on_connect(event: ConnectEvent):
                self.connected = True
                room_id = str(self.client.room_id) if self.client.room_id else None
                import aiosqlite
                from database import DB_PATH
                async with aiosqlite.connect(DB_PATH) as _db:
                    await _db.execute(
                        "UPDATE sessions SET room_id = ? WHERE id = ?",
                        (room_id, self.session_id),
                    )
                    await _db.commit()
                await self.ws_manager.broadcast({
                    "type": "status",
                    "connected": True,
                    "username": self.username,
                    "room_id": room_id,
                    "timestamp": self._now(),
                })
                logger.info(f"Connected to @{self.username} (room: {room_id})")

            # ── Disconnect ───────────────────────────────
            @self.client.on(DisconnectEvent)
            async def on_disconnect(event: DisconnectEvent):
                self.connected = False
                await self.ws_manager.broadcast({
                    "type": "status",
                    "connected": False,
                    "username": self.username,
                    "timestamp": self._now(),
                })

            # ── Live End ─────────────────────────────────
            @self.client.on(LiveEndEvent)
            async def on_live_end(event: LiveEndEvent):
                self.connected = False
                await self.ws_manager.broadcast({
                    "type": "live_end",
                    "username": self.username,
                    "timestamp": self._now(),
                })

            # ── Comment ──────────────────────────────────
            # TikTokLive v7: CommentEvent.content là field thực,
            # CommentEvent.comment là property alias → dùng content để chắc chắn
            @self.client.on(CommentEvent)
            async def on_comment(event: CommentEvent):
                try:
                    # Lấy text: thử content trước, fallback sang comment property
                    text = ""
                    if hasattr(event, "content") and event.content:
                        text = str(event.content)
                    elif hasattr(event, "comment") and event.comment:
                        text = str(event.comment)

                    data = {
                        "type": "comment",
                        "user_id": event.user.unique_id if event.user else "",
                        "username": event.user.unique_id if event.user else "",
                        "nickname": event.user.nickname if event.user else "",
                        "comment": text,
                        "timestamp": self._now(),
                    }
                    await self.db.save_comment(self.session_id, data)
                    await self.ws_manager.broadcast(data)
                except Exception as e:
                    logger.error(f"on_comment error: {e}")

            # ── Gift ─────────────────────────────────────
            # TikTokLive v7: event.streaking = True khi đang trong streak
            # Chỉ lưu khi streak kết thúc (streaking = False)
            @self.client.on(GiftEvent)
            async def on_gift(event: GiftEvent):
                try:
                    # Bỏ qua nếu đang trong streak (tránh lưu trùng)
                    if getattr(event, "streaking", False):
                        return

                    gift = event.gift  # ExtendedGift object
                    data = {
                        "type": "gift",
                        "user_id": event.user.unique_id if event.user else "",
                        "username": event.user.unique_id if event.user else "",
                        "nickname": event.user.nickname if event.user else "",
                        "gift_name": gift.name if gift else "Unknown",
                        "gift_id": str(gift.id) if gift else "",
                        "diamond_count": gift.diamond_count if gift else 0,
                        "repeat_count": getattr(event, "repeat_count", 1) or 1,
                        "timestamp": self._now(),
                    }
                    await self.db.save_gift(self.session_id, data)
                    await self.ws_manager.broadcast(data)
                except Exception as e:
                    logger.error(f"on_gift error: {e}")

            # ── Like ─────────────────────────────────────
            # TikTokLive v7: event.count, event.total
            @self.client.on(LikeEvent)
            async def on_like(event: LikeEvent):
                try:
                    data = {
                        "type": "like",
                        "user_id": event.user.unique_id if event.user else "",
                        "username": event.user.unique_id if event.user else "",
                        "nickname": event.user.nickname if event.user else "",
                        "like_count": getattr(event, "count", 0) or 0,
                        "total_likes": getattr(event, "total", 0) or 0,
                        "timestamp": self._now(),
                    }
                    await self.db.save_like(self.session_id, data)
                    await self.ws_manager.broadcast(data)
                except Exception as e:
                    logger.error(f"on_like error: {e}")

            # ── Join ─────────────────────────────────────
            @self.client.on(JoinEvent)
            async def on_join(event: JoinEvent):
                try:
                    data = {
                        "type": "join",
                        "user_id": event.user.unique_id if event.user else "",
                        "username": event.user.unique_id if event.user else "",
                        "nickname": event.user.nickname if event.user else "",
                        "timestamp": self._now(),
                    }
                    await self.db.save_join(self.session_id, data)
                    await self.ws_manager.broadcast(data)
                except Exception as e:
                    logger.error(f"on_join error: {e}")

            self._task = asyncio.create_task(self.client.start())
            logger.info(f"TikTok client started for @{self.username}")

        except Exception as e:
            logger.error(f"Error starting TikTok client: {e}")
            await self.db.end_session(self.session_id)
            raise

    async def stop(self):
        if self.client:
            try:
                await self.client.disconnect()
            except Exception as e:
                logger.warning(f"Disconnect error: {e}")
        if self._task and not self._task.done():
            self._task.cancel()
            try:
                await self._task
            except asyncio.CancelledError:
                pass
        old_session = self.session_id
        if self.session_id:
            await self.db.end_session(self.session_id)
        self.client = None
        self._task = None
        self.connected = False
        self.session_id = None
        self.username = None
        return old_session

    def get_status(self) -> dict:
        return {
            "connected": self.connected,
            "username": self.username,
            "session_id": self.session_id,
        }
