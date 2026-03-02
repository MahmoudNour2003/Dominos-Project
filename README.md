# 🎲 Domino Game - Multiplayer Network Game

A modern multiplayer domino game built with C# and .NET 10, featuring real-time networking, professional UI, and complete game logic implementation.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [System Requirements](#system-requirements)
- [Project Structure](#project-structure)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Gameplay Instructions](#gameplay-instructions)
- [Architecture](#architecture)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)

---

## 🎯 Overview

**Domino Game** is a networked multiplayer domino game where players can:
- Create or join game rooms
- Play dominoes against other players in real-time
- Track scores across multiple rounds
- Enjoy professional UI with proper tile orientation (horizontal/vertical)
- Automatic win detection and game completion

The project is divided into three main components:
- **DominoServer**: TCP-based game server managing rooms and orchestrating gameplay
- **DominoClient**: Windows Forms client application for player interaction
- **DominoShared**: Shared data models and DTOs for client-server communication

---

## ✨ Features

### Gameplay
- ✅ Standard domino game rules implementation
- ✅ Multiple players per room (2-4 players)
- ✅ Automatic hand distribution and turn management
- ✅ Real-time board updates with proper tile placement
- ✅ Draw and pass mechanics
- ✅ Scoring system with round and total score tracking
- ✅ Automatic game ending when player reaches winning score (default: 20 points)

### UI/UX
- ✅ Professional Windows Forms interface
- ✅ **Dynamic tile orientation**: Doubles display vertically, non-doubles display horizontally
- ✅ Real-time board rendering with centered tile chain
- ✅ Playable/non-playable tile visual indicators (green/red borders)
- ✅ Selection feedback and turn indicators
- ✅ Tooltips and status messages
- ✅ Win/lose notifications with final scores

### Networking
- ✅ TCP socket-based communication
- ✅ JSON serialization for message payload
- ✅ Asynchronous message handling
- ✅ Room-based message broadcasting
- ✅ Connection persistence and error recovery

### Data Management
- ✅ File-based persistence (game rooms and player data)
- ✅ Structured room and player management
- ✅ Game state tracking across sessions

---

## 💻 System Requirements

### Minimum Requirements
- **OS**: Windows 7 or later
- **.NET Runtime**: .NET 10 (Desktop Runtime)
- **RAM**: 512 MB minimum
- **Disk Space**: 100 MB for installation

### Recommended
- **OS**: Windows 10/11
- **RAM**: 2 GB or more
- **CPU**: Any modern processor
- **Network**: Stable TCP connection

---

## 📁 Project Structure

```
DominoGame/
├── DominoServer/                    # Game server (TCP listener + game logic)
│   ├── GameControl/
│   │   └── GameOrchestrator.cs     # Main game orchestration logic
│   ├── Managers/
│   │   ├── PlayerManager.cs        # Player connection & auth
│   │   └── RoomManager.cs          # Room creation & management
│   ├── Networking/
│   │   └── ServerManager.cs        # TCP server & message broadcasting
│   ├── Mocks/
│   │   ├── MockDeck.cs             # Deck implementation
│   │   └── MockRulesEngine.cs      # Game rules
│   ├── Storage/
│   │   └── FileStorage.cs          # Persistence layer
│   ├── ClientHandler.cs            # Individual client connection handler
│   └── Program.cs                  # Server entry point
│
├── DominoClient/                    # Game client (Windows Forms UI)
│   ├── Forms/
│   │   ├── LoginForm.cs            # Login & player authentication
│   │   ├── LobbyForm.cs            # Room browser & creation
│   │   └── GameForm.cs             # Main gameplay interface
│   ├── Controls/
│   │   └── DominoTileControl.cs    # Custom domino tile rendering
│   ├── Networking/
│   │   └── ClientManager.cs        # TCP client & message handling
│   └── Program.cs                  # Client entry point
│
├── DominoShared/                    # Shared models & DTOs
│   ├── DTOs/
│   │   └── NetworkMessage.cs       # Network communication format
│   ├── Engine/
│   │   └── GameState.cs            # Game state model
│   └── Models/
│       ├── DominoCard.cs           # Domino tile representation
│       ├── Player.cs               # Player model
│       └── Room.cs                 # Room model
│
└── README.md                        # This file
```

---

## 🚀 Installation & Setup

### Step 1: Prerequisites
Ensure you have the following installed:
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or any C# IDE

### Step 2: Clone the Repository
```bash
git clone https://github.com/MahmoudNour2003/Dominos-Project.git
cd Dominos-Project
```

### Step 3: Open in Visual Studio
1. Open Visual Studio 2022
2. File → Open → Project/Solution
3. Select the solution file (`.sln`) in the project root
4. Wait for NuGet packages to restore automatically

### Step 4: Build the Solution
```bash
dotnet build
```
Or use Visual Studio:
- Build → Build Solution (Ctrl+Shift+B)

Verify that all three projects build successfully with no errors.

---

## ▶️ Running the Application

### Method 1: Run Both Server and Client (Recommended for Testing)

#### Terminal/PowerShell Method:

**Terminal 1 - Start the Server:**
```bash
cd DominoServer
dotnet run
```

You should see:
```
╔══════════════════════════════════════╗
║    DOMINO GAME SERVER - DEV 1        ║
║    Phase A: TCP Infrastructure       ║
║    Phase B: Player Management        ║
║    Phase C: Room Management          ║
║    Phase D: Game Orchestration       ║
║    Phase E: Persistence              ║
╚══════════════════════════════════════╝

Server listening on 127.0.0.1:5000...
```

**Terminal 2 - Start Client Instance 1:**
```bash
cd DominoClient
dotnet run
```

**Terminal 3 - Start Client Instance 2 (for multiplayer testing):**
```bash
cd DominoClient
dotnet run
```

#### Visual Studio Method:

1. **Set multiple startup projects:**
   - Right-click Solution → Properties
   - Select "Multiple startup projects"
   - Set Start action for:
     - `DominoServer` → Start
     - `DominoClient` → Start
   - Click OK

2. **Press F5** to run all projects simultaneously

### Method 2: Using Docker (Optional)

If Docker is available, you can containerize the server:
```bash
docker build -t domino-server -f DominoServer/Dockerfile .
docker run -p 5000:5000 domino-server
```

---

## 🎮 Gameplay Instructions

### Logging In
1. Run the client application
2. Enter a username (any alphanumeric string)
3. Click "Login"

### Creating/Joining a Room
1. Browse available rooms in the Lobby
2. **To create a new room:**
   - Click "Create Room"
   - Enter room name and select winning score (default: 20)
   - Click "Create"
3. **To join existing room:**
   - Select room from list
   - Click "Join Room"

### Starting a Game
- Once all players are ready, click "Ready" or the host starts the game
- Game begins when all players have 7 dominoes

### Playing the Game
1. **Your Turn:**
   - Click on a domino from your hand
   - Confirm you want to play it
   - If tile fits both ends, select which end (Left/Right)
   - Tile is placed and turn moves to next player

2. **If you can't play:**
   - Click "Draw" to draw from side deck
   - If deck is empty or you draw but still can't play, click "Pass"

3. **Winning:**
   - Empty your hand before others (round winner)
   - Get remaining points from other players' hands
   - First to reach winning score wins the game

### Game Over
- Game ends when any player reaches winning score
- Final scores displayed
- Click "Leave Game" to return to lobby

---

## 🏗️ Architecture

### Network Communication
```
Client ←→ TCP Socket ←→ Server
         JSON Messages
```

**Message Types:**
- `LOGIN` - Player authentication
- `ROOM_LIST_UPDATED` - Available rooms
- `CREATE_ROOM` - Room creation
- `JOIN_ROOM` - Room joining
- `GAME_STARTED` - Game initialization
- `PLAY_CARD` - Player move submission
- `DRAW_CARD` - Draw from deck
- `PASS` - Pass turn
- `GAME_STATE` - Current game state update
- `TURN_CHANGED` - Turn notification
- `GAME_ENDED` - Game completion with winner

### Game Logic Flow
```
Player Login
    ↓
Browse Rooms
    ↓
Join/Create Room
    ↓
Wait for Players
    ↓
Game Start (distribute cards)
    ↓
Turn Loop:
  ├─ Display board & hand
  ├─ Wait for move
  ├─ Validate move
  ├─ Update board
  ├─ Check for round end
  ├─ Calculate scores
  ├─ Check for game end
  └─ Next player
    ↓
Game Over (display winner)
```

### Threading Model
- **Server:** Multi-threaded (one thread per client connection)
- **Client:** UI thread + async networking thread
- **Thread-safe:** All shared state protected by locks

---

## 🛠️ Technologies Used

| Component | Technology | Version |
|-----------|-----------|---------|
| Language | C# | 14.0 |
| Framework | .NET | 10 |
| UI Framework | Windows Forms | .NET 10 |
| Serialization | System.Text.Json | Built-in |
| Networking | TCP Sockets | .NET Sockets |
| Data Storage | File System | JSON files |

---

## 📝 Configuration

### Server Configuration
Edit `DominoServer/appsettings.json` (if exists):
```json
{
  "Server": {
    "Host": "127.0.0.1",
    "Port": 5000
  },
  "Game": {
    "DefaultWinningScore": 20,
    "MaxPlayersPerRoom": 4,
    "InitialHandSize": 7
  }
}
```

### Client Configuration
Server connection details are configured in `ClientManager.cs`:
```csharp
private const string SERVER_ADDRESS = "127.0.0.1";
private const int SERVER_PORT = 5000;
```

To connect to a different server, modify these constants.

---

## 🐛 Troubleshooting

### "Unable to connect to server"
- **Check:** Server is running on correct host/port
- **Check:** Firewall isn't blocking port 5000
- **Fix:** Restart both server and client

### "Username already taken"
- **Cause:** Player already logged in with that username
- **Fix:** Use a different username or wait for timeout

### "Tiles appearing incorrectly"
- **Cause:** Graphics rendering issue
- **Fix:** Ensure Windows graphics drivers are updated
- **Fix:** Run in compatibility mode (Windows 10)

### "Game crashes on play"
- **Cause:** Invalid move was submitted
- **Fix:** Ensure tile matches board ends
- **Check logs:** Check console for error messages

### Build errors
- **Fix 1:** Clean and rebuild solution
  ```bash
  dotnet clean
  dotnet build
  ```
- **Fix 2:** Restore NuGet packages
  ```bash
  dotnet restore
  ```
- **Fix 3:** Ensure .NET 10 SDK is installed
  ```bash
  dotnet --version
  ```

---

## 📊 Game Rules Summary

### Setup
- 28 dominoes in deck (0-6 pips on each end)
- Each player draws 7 dominoes
- Player with highest double goes first

### Gameplay
- Play domino matching one end of board
- If can't play, draw from side deck
- If still can't play, pass turn
- Round ends when one player empties hand

### Scoring
- Round winner: Gets sum of all other players' remaining pips
- Non-winners: Get 0 points for round
- Game winner: First to reach winning score (default: 20)

### Special Cases
- **Double play:** Tile has same value on both ends (displays vertically)
- **Both ends match:** Player chooses which end to play on
- **Domino chain:** Tiles expand left and right from initial placement
- **Empty deck:** Players must pass if they draw last tile and can't play

---

## 📞 Support & Contact

For issues, bugs, or feature requests:
- Check existing [GitHub Issues](https://github.com/MahmoudNour2003/Dominos-Project/issues)
- Create new issue with detailed description
- Include error messages and steps to reproduce

---

## 📄 License

This project is part of an educational assignment. Usage subject to course requirements.

---

## 👨‍💻 Developers

- **Mahmoud Nour** - Project Lead
- Developed as part of ITI C# Course Final Project

---

## 🎓 Educational Purpose

This project demonstrates:
- ✅ Network programming (TCP sockets)
- ✅ Asynchronous programming (async/await)
- ✅ Game state management
- ✅ Windows Forms UI design
- ✅ Event-driven architecture
- ✅ Thread-safe concurrent programming
- ✅ JSON serialization/deserialization
- ✅ File-based data persistence
- ✅ Game logic implementation
- ✅ Professional software design patterns

---

## 🚀 Future Enhancements

Potential features for future versions:
- [ ] Web-based UI (ASP.NET Core)
- [ ] Database persistence (SQL Server)
- [ ] Player statistics and leaderboards
- [ ] Chat system
- [ ] Game replays
- [ ] AI opponents
- [ ] Mobile app (MAUI)
- [ ] Spectator mode
- [ ] Tournament support
- [ ] Voice chat integration

---

**Happy Gaming! 🎉**

Last Updated: 2024
Version: 1.0.0
