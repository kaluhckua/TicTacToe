/// <reference path="http-requester.js" />
/// <reference path="class.js" />
/// <reference path="http://crypto-js.googlecode.com/svn/tags/3.1.2/build/rollups/sha1.js" />
var persisters = (function () {
    var ticTacToeHub;
    var nickname = localStorage.getItem("nickname");
    var sessionKey = localStorage.getItem("sessionKey");
    var wrapper;
    function saveUserData(userData) {
        localStorage.setItem("nickname", userData.Nickname);
        localStorage.setItem("sessionKey", userData.SessionKey);
        nickname = userData.Nickname;
        sessionKey = userData.SessionKey;
    }
    function clearUserData() {
        localStorage.removeItem("nickname");
        localStorage.removeItem("sessionKey");
        nickname = null;
        sessionKey = null;
    }
    ticTacToeHub = $.connection.ticTacToeHub;

    var MainPersister = Class.create({

        init: function (selector) {
            wrapper = $(selector);
            this.user = new UserPersister();
            this.game = new GamePersister();
        },
        isUserLoggedIn: function () {
            var isLoggedIn = nickname != null && sessionKey != null;
            return isLoggedIn;
        },
        nickname: function () {
            return nickname;
        },
        sessionKey: function () {
            return sessionKey;
        }
    });

    var UserPersister = Class.create({
        init: function () {
        },
        login: function (user, success) {
            var userData = {
                username: user.username,
                authCode: CryptoJS.SHA1(user.username + user.password).toString()
            };
            ticTacToeHub.server.login(userData).done(function (userLogged) {
                saveUserData(userLogged);
                success();
            })
        },
        register: function (user, success) {
            var userData = {
                username: user.username,
                nickname: user.nickname,
                authCode: CryptoJS.SHA1(user.username + user.password).toString()
            };
            ticTacToeHub.server.registerAndReturnLoggedUser(userData).done(function (userLogged) {
                saveUserData(userLogged);
                success();
            })
        },
        registerConnectionId: function () {
            ticTacToeHub.server.saveConnectionIdBySessionKey(sessionKey);
        },
        logout: function (success) {
            ticTacToeHub.server.logout(sessionKey).done(function () {
                clearUserData();
                success();
            })
        },
    });

    var GamePersister = Class.create({
        init: function () {
        },
        open: function () {
            ticTacToeHub.server.returnOpenGamesToClient(sessionKey);
        },
        active: function () {
            ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
        },
        created: function () {
            ticTacToeHub.server.returnCreatedGamesToClient(sessionKey)
        },
        joined: function () {
            ticTacToeHub.server.returnJoinedGamesToClient(sessionKey);
        },
        create: function (game, success) {
            var createGameModel = {
                title: game.title,
                sessionkey: sessionKey,
            }
            if (game.password) {
                createGameModel.password = CryptoJS.SHA1(game.password).toString();
            }
            ticTacToeHub.server.createGameAndNotifyOthersClients(createGameModel).done(function () {
                success();
            });
        },
        join: function (game, success) {
            var self = this;
            var joinGameModel = {
                gameid: game.gameId,
                sessionkey: sessionKey,
            }
            if (game.password) {
                joinGameModel.password = CryptoJS.SHA1(game.password).toString();
            }
            ticTacToeHub.server.joinGameAndNotifyMyOpponent(joinGameModel).done(function () {
                success();
            });

        },
        isGamePassword: function (gameId, success) {          
            ticTacToeHub.server.checkGamePassword(gameId).done(function (isPassword) {
                success(isPassword);
            });
        },
        start: function (gameId, success) {
            ticTacToeHub.server.startGameAndNotifyMyOpponent(gameId, sessionKey).done(function (gameState) {
                success(gameState);
            })
        },
        guess: function (guess) {
            guess.sessionKey = sessionKey;
            ticTacToeHub.server.makeGuess(guess);
        },
        reload: function (gameId, success) {
            ticTacToeHub.server.reloadGameAndNotifyMyOpponent(gameId, sessionKey).done(function () {
                ticTacToeHub.server.startGameAndNotifyMyOpponent(gameId, sessionKey).done(function (gameState) {
                    success(gameState);
                });
            });
        },
        update: function (gameId) {
            ticTacToeHub.server.updateGame(gameId, sessionKey);
        },
        leave: function (gameId, success) {
            ticTacToeHub.server.leaveGameAndRestartState(gameId, sessionKey).done(function () {
                success();
            });
        },
        deleteGame: function (gameId, success) {
            ticTacToeHub.server.deleteGameAndNotifyMyOpponent(gameId, sessionKey).done(function () {
                success();
            });
        }
    });

    ticTacToeHub.client.updateOpenGamesList = function (games) {        
        var list = ui.openGamesList(games);
        wrapper.find("#open-games").html(list);
    }
    ticTacToeHub.client.updateActiveGamesList = function (games) {
        var list = ui.activeGamesList(games, nickname);
        wrapper.find("#active-games").html(list);
    }
    ticTacToeHub.client.updateCreatedGamesList = function (games) {
        var list = ui.createdGamesList(games);
        wrapper.find("#created-games").html(list);
    }
    ticTacToeHub.client.updateJoinedGamesList = function (games) {
        var list = ui.joinedGamesList(games);
        wrapper.find("#joined-games").html(list);
    }
    ticTacToeHub.client.createdNewGame = function () {       
        ticTacToeHub.server.returnOpenGamesToClient(sessionKey);
        ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
    }
    ticTacToeHub.client.joinedGame = function () {
        ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
        ticTacToeHub.server.returnCreatedGamesToClient(sessionKey);
    };
    ticTacToeHub.client.leftGame = function (gameState) {
        ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
        ticTacToeHub.server.returnCreatedGamesToClient(sessionKey);
        var game = wrapper.find("#game-holder");
        if (game.attr("data-game-id") == gameState.GameId) {
            game.html("");
            game.attr("data-game-id", "");
        }
        var messageHolder = wrapper.find("#messages-holder");
        messageHolder.html(gameState.Opponent + ' left the game "' +
                 gameState.Title + '"<br/>');
        if (gameState.Winner != null) {
            messageHolder.append('The winner is ' + gameState.Winner);
        }
        setTimeout(function () {
            messageHolder.html("");
        }, 3000);




    };
    ticTacToeHub.client.startedGame = function (gameState) {
        var game = wrapper.find("#game-holder");
        if (game.attr("data-game-id") == "" || game.attr("data-game-id") == gameState.GameId) {
            var gameHtml = ui.ticTacToe(gameState);
            game.html("");
            game.append(gameHtml).attr("data-game-id", gameState.GameId);

        }
        ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
    };
    ticTacToeHub.client.deletedGame = function (gameState) {
        var game = wrapper.find("#game-holder");
        if (game.attr("data-game-id") == "" || game.attr("data-game-id") == gameState.GameId) {
            game.html("");
        }
        ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
        ticTacToeHub.server.returnJoinedGamesToClient(sessionKey);

        var messageHolder = wrapper.find("#messages-holder");
        messageHolder.html(gameState.Opponent + ' deleted the game "' +
                 gameState.Title + '"<br/>');
        if (gameState.Winner != null) {
            messageHolder.append('The winner is ' + gameState.Winner);
        }
        setTimeout(function () {
            messageHolder.html("");
        }, 3000);


    };
    ticTacToeHub.client.addMarkerPlacement = function (gameId, symbol, position) {
        var gameHolderId = wrapper.find("#game-holder").attr("data-game-id");
        if (gameHolderId == gameId) {
            if (symbol == "O") {
                wrapper.find("#game-holder #game .box#" + position).addClass("markO").addClass("marked");
            }
            else {
                wrapper.find("#game-holder #game .box#" + position).addClass("markX").addClass("marked");
            }
        }


    };
    ticTacToeHub.client.waitingForOpponent = function (gameId, message) {
        var gameHolderId = wrapper.find("#game-holder").attr("data-game-id");
        if (gameHolderId == gameId) {
            wrapper.find("#game-holder #game-information #on-turn").html("");
            wrapper.find("#game-holder #game-information #on-turn").html("<strong>Waiting for the opponent to " + message + "!</strong>");
        }

    };
    ticTacToeHub.client.waitingForMarkerPlacement = function (gameId) {
        var gameHolderId = wrapper.find("#game-holder").attr("data-game-id");
        if (gameHolderId == gameId) {
            wrapper.find("#game-holder #game-information #on-turn").html("");
            wrapper.find("#game-holder #game-information #on-turn").html("<strong>Your turn!</strong>");
        }

    };
    ticTacToeHub.client.updateGameUI = function (gameState) {
        var gameHtml = ui.ticTacToe(gameState);
        var game = wrapper.find("#game-holder");
        game.html("");
        game.append(gameHtml).attr("data-game-id", gameState.GameId);

        if (gameState.X != null) {
            for (var i = 0; i < gameState.X.length; i++) {
                wrapper.find("#game .box#" + gameState.X[i]).addClass("markX").addClass("marked");

            }
        }
        if (gameState.O != null) {
            for (var i = 0; i < gameState.O.length; i++) {
                wrapper.find("#game .box#" + gameState.O[i]).addClass("markO").addClass("marked");
            }
        }
        if (gameState.State == "Finished") {
            wrapper.find("#game-holder #game-information #on-turn").html("");
            wrapper.find("#game-holder #game-information #btn-reload-game").css("display", "block");
        }
        if (gameState.Winner != null) {
            var html = 'End of game!';
            html += ' The winner is: ' + gameState.Winner;
            wrapper.find("#game-holder #game-information #message").html(html);

        }
    };
    ticTacToeHub.client.gameOver = function (gameState) {

        var html = 'End of the game ' + gameState.Title + '!';
        if (gameState.Winner != null) {
            html += ' The winner is: ' + gameState.Winner;
        }
        wrapper.find("#messages-holder").html(html);
        ticTacToeHub.server.returnCreatedGamesToClient(sessionKey);
        ticTacToeHub.server.returnActiveGamesToClient(sessionKey);
        var gameHolderId = wrapper.find("#game-holder").attr("data-game-id");
        if (gameHolderId == gameState.GameId) {
            wrapper.find("#game-holder #game-information #on-turn").html("");
        }

    };
    ticTacToeHub.client.reloadedGame = function () {
        wrapper.find("#messages-holder").html("");
    };
    ticTacToeHub.client.restartOption = function (gameId) {
        var gameHolderId = wrapper.find("#game-holder").attr("data-game-id");
        if (gameHolderId == gameId) {
            wrapper.find("#game-holder #game-information #on-turn").html("");
            wrapper.find("#game-holder #game-information #btn-reload-game").css("display", "block");
        }

    }
    ticTacToeHub.client.nothifyAllClientForOpenGames = function () {
        ticTacToeHub.server.returnOpenGamesToClient(sessionKey);
    }
    ticTacToeHub.client.serverErrorException = function (errorMessage) {
        var errorMessageHolder = $("#error-message-holder");
        errorMessageHolder.html(errorMessage);
        setTimeout(function () {
            errorMessageHolder.html("");
        }, 3000);

    }
    return {
        get: function (selector) {
            return new MainPersister(selector);
        }
    };

}());