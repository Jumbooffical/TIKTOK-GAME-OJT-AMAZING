const { WebcastPushConnection } = require('tiktok-live-connector');
const { WebSocketServer } = require('ws');

const USERNAME = 'your_tiktok_username'; // must be live
const wss = new WebSocketServer({ port: 8080 });

function send(obj) {
	const msg = JSON.stringify(obj);
	for (const client of wss.clients) {
		if (client.readyState === 1) client.send(msg);
	}
}

const tiktok = new WebcastPushConnection(USERNAME);
tiktok.connect().then(() => console.log('Connected to live'));

tiktok.on('chat', d => send({ type: 'chat', user: d.uniqueId, comment: d.comment }));
tiktok.on('like', d => send({ type: 'like', user: d.uniqueId, count: d.likeCount }));
tiktok.on('follow', d => send({ type: 'follow', user: d.uniqueId }));
tiktok.on('gift', d => {
	// streakable gifts (giftType 1) fire repeatedly; only send once the streak ends
	if (d.giftType === 1 && !d.repeatEnd) return;
	send({ type: 'gift', user: d.uniqueId, gift: d.giftName, count: d.repeatCount, diamonds: d.diamondCount });
});