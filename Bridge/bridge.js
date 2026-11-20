// USAGE:
// node bridge.js yourusername

const { TikTokLiveConnection, WebcastEvent } = require('tiktok-live-connector');
const { WebSocketServer } = require('ws');

// Change 'your_tiktok_username' to your username
const USERNAME = process.argv[2] || 'your_tiktok_username';

const connection = new TikTokLiveConnection(USERNAME, {});
const wss = new WebSocketServer({ port: 8080 });

function send(obj) {
	const msg = JSON.stringify(obj);
	for (const client of wss.clients) {
		if (client.readyState === 1) client.send(msg);
	}
}

// the 2.x payload may nest the user: d.user.uniqueId instead of d.uniqueId
const who = d =>
    d.user?.nickname ||
    d.user?.uniqueId ||
    d.nickname ||
    d.uniqueId ||
    'unknown';

connection.connect()
	.then(state => console.log('Connected to room', state.roomId))
	.catch(err => console.error('Failed to connect:', err));

connection.on(WebcastEvent.CHAT, d => {
	const comment =
        d.comment ??
        d.content ??
        d.text ??
        '';
	console.log('chat', who(d), comment);
	send({ type: 'chat', user: who(d), comment: comment });
});

connection.on(WebcastEvent.LIKE, d => {
	send({ type: 'like', user: who(d), count: d.likeCount });
});

connection.on(WebcastEvent.FOLLOW, d => {
	send({ type: 'follow', user: who(d) });
});

connection.on(WebcastEvent.GIFT, d => {
	console.log('gift raw:', JSON.stringify(d).slice(0, 300)); // check the field names once
	send({
		type: 'gift',
		user: who(d),
		gift: d.giftName,
		count: d.repeatCount,
		diamonds: d.diamondCount
	});
});