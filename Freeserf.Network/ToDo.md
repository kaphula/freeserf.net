﻿- When server closes or a client disconnects the connection should not be closed immediately
  because the other side has no chance to retrieve the disconnect message then.
  Maybe wait for disconnect responses for a given timeout.
- Outro / game end / surrender
- Test client and server shutdowns in different states
- Call all user actions
- Check exceptions
- Longrun tests
- Server game updates (atm the client sees no game updates beside his own)
- No heartbeats are sent while in lobby nor are timeouts checked in lobby
- Server does not check for client timeouts yet
- When a server game is started (enter game init mp server screen is enough) and then a game as client is joined, there are several bugs (seems that the client has still server view)
