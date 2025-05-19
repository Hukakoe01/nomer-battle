const express = require('express');
const app = express();
const cors = require('cors');
const PORT = 3000;

app.use(cors());
app.use(express.json());

let gameState = {
    players: [],
    turn: 0,
    actions: [],
    chat: [],
    board: {}
};

// Игрок подключается
app.post('/join', (req, res) => {
    if (gameState.players.length >= 2) {
        return res.status(400).json({ message: 'Комната заполнена' });
    }

    const playerId = gameState.players.length;
    gameState.players.push({ id: playerId });
    res.json({ playerId });
});

// Игрок делает ход
app.post('/move', (req, res) => {
    const { playerId, moveData } = req.body;

    if (gameState.turn !== playerId) {
        return res.status(403).json({ message: 'Сейчас не ваш ход' });
    }

    gameState.actions.push(moveData);
    gameState.turn = (gameState.turn + 1) % 2;

    res.json({ success: true });
});

// Получить текущие данные
app.get('/state', (req, res) => {
    res.json(gameState);
});

// Добавить в чат
app.post('/chat', (req, res) => {
    const { playerId, message } = req.body;
    gameState.chat.push({ playerId, message });
    res.json({ success: true });
});

// Сброс игры (удобно для отладки)
app.post('/reset', (req, res) => {
    gameState = {
        players: [],
        turn: 0,
        actions: [],
        chat: [],
        board: {}
    };
    res.json({ success: true });
});

app.listen(PORT, () => {
    console.log(`Сервер запущен на http://localhost:${PORT}`);
});
