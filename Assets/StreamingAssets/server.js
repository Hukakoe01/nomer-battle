const express = require('express');
const cors = require('cors');
const app = express();
const PORT = 3000;

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

let gameState = {
    players: [],
    actions: [null, null],
    chat: [],
    board: {},
    hostIP: null,
    currentTurn: 0
};

app.use((req, res, next) => {
    res.setHeader('Content-Type', 'application/json; charset=utf-8');
    next();
});

app.post('/join', (req, res) => {
    const name = req.body.playerName;
    const clientIP = req.ip || req.connection.remoteAddress;

    if (gameState.players.length === 0) {
        gameState.hostIP = clientIP;
    }
    else if (clientIP === gameState.hostIP) {
        return res.status(400).json({ error: 'cannot_connect_to_self' });
    }

    if (!name || typeof name !== 'string' || name.trim().length === 0) {
        return res.status(400).json({ error: 'invalid_name' });
    }

    if (gameState.players.length >= 2) {
        return res.status(400).json({ error: 'room_full' });
    }

    const nameExists = gameState.players.some(p => p.name === name);
    if (nameExists) {
        return res.status(400).json({ error: 'already_joined' });
    }

    const playerId = gameState.players.length;
    gameState.players.push({ id: playerId, name, hp: 100 });

    if (gameState.players.length === 2) {
        const firstPlayer = gameState.players[0]?.name || "Player 1";
        gameState.chat.push(`Player 1: ${firstPlayer}`);
        gameState.chat.push(`Opponent joined: ${name}`);
    }

    res.json({ playerId });
});

app.post('/move', (req, res) => {
    let playerId = parseInt(req.body.playerId);
    const damage = parseInt(req.body.damage);

    if (isNaN(playerId) || isNaN(damage)) {
        return res.status(400).json({ error: 'invalid_move_data' });
    }

    if (playerId !== gameState.currentTurn) {
        return res.status(400).json({ error: 'not_your_turn' });
    }

    if (gameState.actions[playerId] !== null) {
        return res.status(400).json({ error: 'already_moved' });
    }

    gameState.actions[playerId] = damage;

    const targetId = (playerId + 1) % 2;
    gameState.players[targetId].hp -= damage;
    if (gameState.players[targetId].hp < 0) {
        gameState.players[targetId].hp = 0;
    }

    gameState.chat.push(`${gameState.players[targetId].name} получил урон: ${damage}`);

    gameState.actions[playerId] = null;
    gameState.currentTurn = targetId;

    console.log(`Игрок ${playerId} атаковал на ${damage}. Следующий ход: Игрок ${targetId}`);
    res.json({ success: true });
});

app.get('/state', (req, res) => {
    res.json(gameState);
});

app.post('/chat', (req, res) => {
    const playerId = parseInt(req.body.playerId);
    const message = req.body.message;

    if (playerId === undefined || message === undefined) {
        return res.status(400).json({ error: 'invalid_data' });
    }

    const playerName = gameState.players[playerId]?.name || "Player";
    gameState.chat.push(`${playerName}: ${message}`);

    if (gameState.chat.length > 50) {
        gameState.chat = gameState.chat.slice(-50);
    }

    console.log(`Сообщение от ${playerName}: ${message}`);
    res.json({ success: true });
});

app.post('/reset', (req, res) => {
    gameState = {
        players: [],
        actions: [null, null],
        chat: [],
        board: {},
        hostIP: null,
        currentTurn: 0
    };
    console.log("Игра сброшена.");
    res.json({ success: true });
});

app.post('/shutdown', (req, res) => {
    console.log('Запрошено завершение работы сервера...');
    res.set('Connection', 'close');
    res.status(200).send({ message: 'Сервер выключается...' });

    server.close(() => {
        console.log('Сервер остановлен');
        process.exit(0);
    });

    setTimeout(() => {
        console.error('Принудительное завершение...');
        process.exit(1);
    }, 2000);
});

const server = app.listen(PORT, () => {
    console.log(`Сервер запущен на http://localhost:${PORT}`);
});

process.on('SIGINT', () => {
    console.log('Получен SIGINT. Остановка сервера...');
    server.close(() => {
        console.log('Сервер остановлен. Выход из процесса...');
        process.exit(0);
    });
});