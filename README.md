  Dark Hero — Semester Project
===============================

Author: Anna Shabosova  
Year: 2025  
Platform: macOS / Windows (depending on build)

-------------------------------
 📖 About the Game:
-------------------------------
Dark Hero is a 2D action roguelike game featuring procedural level generation with branching paths, puzzle-like mechanics, and parallel world switching.  
The player explores dynamically generated levels, collects loot, interacts with various NPCs through branching dialogues, and fights a boss guarding the final exit.

The project was developed as a semester assignment for university and is designed for easy extension thanks to its 
modular structure and prefab-based implementation for levels, loot, and enemies.

-------------------------------
 🛠️ Game Features:
-------------------------------
- **Procedural level generation** with branching paths and puzzle-like gates between worlds. Each branch ends with a dead end.
- **World switching system** allowing the player to shift between two parallel dimensions, each with unique level layouts and interactions.
- **Dynamic loot and background generation** for each level.
- **Interactive dialogues** with multiple choices affecting progression:
  - Dialogue with **Panda NPC** including three dialogue branches and quest rewards.
  - Dialogue with a **Merchant NPC** for accessing an in-game shop.
  - Dialogue with a **Boss NPC** offering a riddle before the fight — a correct answer defeats the boss instantly.
- **Shop system** with three items:
  1. Health potion.
  2. +Damage buff for 10 seconds.
  3. Invincibility for 10 seconds.
- **Loot system** with four types of interactable objects: two breakable and two collectible.
- **Save and load system** for level states, world switches, player stats, and loot collected.
- **Subtitle system** for dialogues and events.
- **Sound effects and camera shake** for hits and explosions.
- **Easy expandability** using custom prefabs:
  - 15 level room prefabs.
  - 4 loot prefabs.
  - 3 enemy prefabs.

-------------------------------
 🎮 Controls:
-------------------------------
W / A / S / D — Move  
Left Mouse Button — Attack  
E — Interact / Dialog  
Esc — Pause / Menu  

-------------------------------
 🎥 About the Demo Version:
-------------------------------
For the purpose of presentation and project review, a special demo build has been prepared.  
This version includes all core mechanics but with adjusted parameters for quicker and smoother demonstration:

- **Minimal level generation** — reduced number of rooms and branches.
- **Reduced boss HP** — for a faster boss fight.
- **Lower enemy damage** — for easier playthrough.
- Full version includes extended generation, full difficulty, and complete puzzles.

-------------------------------
 📧 Contact:
-------------------------------
annashabossova@gmail.com

-------------------------------
 📌 Notes:
-------------------------------
This project was developed in Unity with C#.  
It was tested and built on macOS and Windows platforms.

Sound effects, sprites, and tutorials were sourced from open libraries and free resources.  

Thank you for playing!
