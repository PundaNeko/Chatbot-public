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

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({
  extended: true 
})); 
app.use((req, res, next) => {
  console.log(`${req.method} ${req.path} - body:`, req.body);
  next();
});

//database
const dbConfig = {
    host: process.env.DB_HOST,
    user: process.env.DB_USER,
    password: process.env.DB_PASS,
    database: process.env.DB_NAME
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

app.post('/enter-name', async(req, res) =>{
    console.log('ENTER-NAME HIT:', req.body);  // Add this
    const {username} = req.body;
    const db = await getDb();

    const[rows] = await db.execute('SELECT id FROM users WHERE username = ?', [username]);
    if(rows[0]){
        //yay, found someone
        res.json({userId:rows[0].id, username, exists: true});
    } else{
        const [result] = await db.execute('INSERT INTO users (username) VALUES (?)', [username]);
        res.json({userId: result.insertId, username, exists: false});
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
    socket.on('join-session', async ({sessionId, userId}) =>{
        try
        {
            //send messages
            const user = await getUserById(userId);
            if(!user) {
                socket.emit('Error: user not found');
                return;
            }
    
            socket.username = user.username;
            socket.userId = user.id;
    
            userSessions.set(socket.id, {
                sessionId,
                userId: user.id,
                username: user.username
            });
            socket.join(sessionId);
            
    
            const db = await getDb();
            const[rows] = await db.execute(
                'SELECT m.*, u.username FROM messages m JOIN users u ON m.sender_id = u.id WHERE session_id = ? ORDER BY created_at DESC LIMIT 50',
                [sessionId]
            );
            socket.emit('load-messages', rows.reverse());
        }
        catch (error){
            console.error('Error joining session: ', error);
        }
    });
    
    
    //send current
    socket.on('send-message', async({sessionId, content}) => {
        const session = userSessions.get(socket.id);
        console.log('Session found:', session);

        if(!session) return;

        const db = await getDb();

        const [result] = await db.execute(
            'INSERT INTO messages (session_id, sender_id, content) VALUES (?, ?, ?)',
            [sessionId, session.userId, content]
        );
        
        const message = {
            id: result.insertId,
            session_id: sessionId,
            sender_id: session.userId,
            content, 
            created_at: new Date().toISOString()
        };
        
        const username = session.username|| socket.username || 'Anonymous';

        console.log('your username: ', username);
        
        io.to(sessionId).emit('new-message', {username, content});

    });
    
    socket.on('disconnect', () =>{
        userSessions.delete(socket.id);
        console.log('User disconnected:', socket.id);
    });
});


server.listen(process.env.PORT || 3000, () => {
    console.log('Server running on port', process.env.PORT || 3000);
});

async function getUserById(id){
    const db = await getDb();
    const [rows] = await db.execute(
        'SELECT * FROM users WHERE id = ?',
        [id]
    );
    return rows[0] || null;
}