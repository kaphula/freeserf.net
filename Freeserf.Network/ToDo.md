﻿- When server closes or a client disconnects the connection should not be closed immediately
  because the other side has no chance to retrieve the disconnect message then.
  Maybe wait for disconnect responses for a given timeout.
- Outro / game end / surrender
- Test client and server shutdowns in different states
- Call all user actions
- Check exceptions
- Longrun tests
- Fix server game updates
- No heartbeats are sent while in lobby nor are timeouts checked in lobby
- Server does not check for client timeouts yet
- When a server game is started (enter game init mp server screen is enough) and then a game as client is joined on same app instance, there are several bugs (seems that the client has still server view)
- Full syncs
    - Client still keeps his base game (renderview, interface, base map data, map change handler, etc)
    - Client must first delete all game objects / render objects
    - Client must not rely on changeObjects in map but has to update each tile (remove old stuff, place new stuff)
    - To do all this the client has to know that it is a full sync
        - First sync is always full but can be treated as an update
- If something goes wrong on client side
    - Pause game
    - Disable all input
    - Send a disconnect to the server
    - Close all previous popups
    - Open a popup with the error message or at least "Error ..."
    - Limit user input to this popup (disable viewport input)
    - After closing this popup open game init box (and close current game)
- Syncs take too long
    - If game time differs too much after sync, a partial game state update request would be nice
