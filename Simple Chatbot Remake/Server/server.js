require('dotenv').config();
const express = require('express');
const http = require('http');
const socketio = require('socket.io');
const cors = require('cors');
const mysql =  require('mysql2/promise');
const { execArgv } = require('process');

const app = express();
const server = http.createServer(app);
const io = socketio(server, {cors: {origin: "*"}});

app.arguments(cors());
app.arguments(express.json());

//database
const dbConfig = {
    host: process.env.DB_HOST,
    user: process.env.DB_USER,
    password: process.env.DB_PASS,
    daatabase: process.env.DB_NAME
}

const userSessions = new Map();

async function getDb(){
    return mysql.createPool(dbConfig);
}

app.post('/register', async(req, res) =>{
    const {username} = req.body;
    const db = await getDb();
    try{
        const{result} = await db.execute(

            'INSERT INTO users (username) VALUES (?)',
            [username]
        );
        res.json({userId: result.insertId, username});
    }
    catch (err){
        res.status(400).json({ error: 'Username Taken'})
    }
});
app.get('/messages/:sessionId', async (req, res) => {
    const {sessionId} = req.params;
    const db = await getDb();
    const [rows] = await db.execute (
        "SELECT m.*, u.username FROM messages m JOIN users u ON m.sender_id = u.id WHERE session_id = ? ORDER BY created_at LIMIT 50",
        [sessionId]
    );
    res.json(rows);
});

//Websocket logic
io.on('connection', (socket) =>{
    console.log('User connected: ', socket.id);
    
    //join session
    socket.on('join-session', async ({sessionId, userId, username}) =>{
        socket.join(sessionId);
        userSessions.set(socketId, {sessionId, userId});
        
        //send messages
        const db = await getDb();
        const[rows] = await db.execute(
            'SELECT m.*, u.username FROM messages m JOIN users u ON m.sender_id = u.id WHERE session_id = ? ORDER BY created_at DESC LIMIT 50',
            [sessionId]
        );
    });
    
    socket.emit('load-messages', rows.reverse());
    
    //send current
    socket.on('send-message', async({sessionId, content}) => {
        const session = userSessions.get(socket.id);
        if(!session) return;
    
        const db = await getDb();
        const [result] = await db.execute(
            'INSERT INTO messages (session_id, sender_id, content VALUES (?,?,?)',
            [sessionId, session.userId, content]
        );
    
        const message = {
            id: result.insertId,
            session_id: sessionId,
            sender_id: session.userId,
            content, 
            created_at: new Date().toISOString()
        };
        io.to(sessionId).emit('new-message', message);
    });
    
    socket.on('disconnect', () =>{
        userSessions.delete(socket.id);
        console.log('User disconnected:', socket.id);
    });
});


server.listen(process.env.PORT || 3000, () => {
    console.log('Server running on port', process.env.PORT || 3000);
});