# 🧰 Mirror Server-Authoritative Inventory System

This repository contains a **modular** and **fully server-authoritative** inventory system designed for multiplayer games built with **Unity + Mirror**.

It's lightweight, easy to integrate, and strictly separates client-side visuals from server-side logic for cheat prevention and reliable synchronization.

---

## ✨ Features

- 🔐 **Server-authoritative architecture**: All actions (add, move, remove) are handled and validated on the server.
- 📦 **Dynamic containers**: Add/remove inventory containers on the fly (e.g. pockets, backpacks, chests…).
- 🧱 **Grid-based slots**: Each container is a 2D grid. No Tetris-style shapes or rotations.
- ♻️ **Smart stacking**: Automatically stacks items if possible (up to max stack size).
- 🖱️ **Drag & drop movement**: Intuitive client-side interface with visual ghost slot.
- 🔄 **Efficient sync**: Only updates changed slots, not the entire inventory.
- 🧪 **Debug shortcuts**: Quick testing with keyboard inputs to add/remove containers in runtime.
- 🛠️ **Custom Editor Tools**: Includes an in-Editor tool to **give items** to players and **visualize inventory states** in real-time.

---

## 🚧 Current Limitations

- ❌ No persistent save/load (no JSON or database integration yet).
- ❌ No item rotation or shapes (not a Tetris-style system).
- ❌ No container permission system (e.g. for shared storage).
- ❌ No client feedback on failed actions (e.g. no room available).

---

## 🔧 Requirements

- **Unity 6000.0.34f1**
- **Mirror 96.0.1**
- A basic item database using ScriptableObjects (`ItemSO`) for defining item stats

---

## 📜 License

This project is licensed under the **MIT License** – free to use in commercial or non-commercial projects.  
Attribution is appreciated but not required.
